/* The copyright in this software is being made available under the BSD
 * License, included below. This software may be subject to other third party
 * and contributor rights, including patent rights, and no such rights are
 * granted under this license.
 *
 * Copyright (c) 2010-2020, ITU/ISO/IEC
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *  * Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 *  * Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 *  * Neither the name of the ITU/ISO/IEC nor the names of its contributors may
 *    be used to endorse or promote products derived from this software without
 *    specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS
 * BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */

/** \file     TDecSlice.cpp
    \brief    slice decoder class
*/

#include "TDecSlice.h"
#include "TDecConformance.h"

//! \ingroup TLibDecoder
//! \{

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

TDecSlice::TDecSlice()
{
}

TDecSlice::~TDecSlice()
{
}

Void TDecSlice::create()
{
}

Void TDecSlice::destroy()
{
}

Void TDecSlice::init(TDecEntropy* pcEntropyDecoder, TDecCu* pcCuDecoder, TDecConformanceCheck *pDecConformanceCheck)
{
  m_pcEntropyDecoder     = pcEntropyDecoder;
  m_pcCuDecoder          = pcCuDecoder;
  m_pDecConformanceCheck = pDecConformanceCheck;
}

// hevc_demo
Void TDecSlice::decompressSlice(TComInputBitstream** ppcSubstreams, TComPic* pcPic, TDecSbac* pcSbacDecoder, ofstream& cupuOutput, ofstream& predictionOutput, ofstream& intraOutput, ofstream& motionVectorsOutput)
{
  TComSlice* pcSlice                 = pcPic->getSlice(pcPic->getCurrSliceIdx());

  const Int  startCtuTsAddr          = pcSlice->getSliceSegmentCurStartCtuTsAddr();
  const Int  startCtuRsAddr          = pcPic->getPicSym()->getCtuTsToRsAddrMap(startCtuTsAddr);
  const UInt numCtusInFrame          = pcPic->getNumberOfCtusInFrame();

  const UInt frameWidthInCtus        = pcPic->getPicSym()->getFrameWidthInCtus();
  const Bool depSliceSegmentsEnabled = pcSlice->getPPS()->getDependentSliceSegmentsEnabledFlag();
  const Bool wavefrontsEnabled       = pcSlice->getPPS()->getEntropyCodingSyncEnabledFlag();

  m_pcEntropyDecoder->setEntropyDecoder ( pcSbacDecoder  );
  m_pcEntropyDecoder->setBitstream      ( ppcSubstreams[0] );
  m_pcEntropyDecoder->resetEntropy      (pcSlice);

  // decoder doesn't need prediction & residual frame buffer
  pcPic->setPicYuvPred( 0 );
  pcPic->setPicYuvResi( 0 );

#if ENC_DEC_TRACE
  g_bJustDoIt = g_bEncDecTraceEnable;
#endif
  DTRACE_CABAC_VL( g_nSymbolCounter++ );
  DTRACE_CABAC_T( "\tPOC: " );
  DTRACE_CABAC_V( pcPic->getPOC() );
  DTRACE_CABAC_T( "\n" );

#if ENC_DEC_TRACE
  g_bJustDoIt = g_bEncDecTraceDisable;
#endif

  // The first CTU of the slice is the first coded substream, but the global substream number, as calculated by getSubstreamForCtuAddr may be higher.
  // This calculates the common offset for all substreams in this slice.
  const UInt subStreamOffset=pcPic->getSubstreamForCtuAddr(startCtuRsAddr, true, pcSlice);


  if (depSliceSegmentsEnabled)
  {
    // modify initial contexts with previous slice segment if this is a dependent slice.
    const UInt startTileIdx=pcPic->getPicSym()->getTileIdxMap(startCtuRsAddr);
    const TComTile *pCurrentTile=pcPic->getPicSym()->getTComTile(startTileIdx);
    const UInt firstCtuRsAddrOfTile = pCurrentTile->getFirstCtuRsAddr();

    if( pcSlice->getDependentSliceSegmentFlag() && startCtuRsAddr != firstCtuRsAddrOfTile)
    {
      if ( pCurrentTile->getTileWidthInCtus() >= 2 || !wavefrontsEnabled)
      {
        pcSbacDecoder->loadContexts(&m_lastSliceSegmentEndContextState);
      }
    }
  }

  // for every CTU in the slice segment...

  Bool isLastCtuOfSliceSegment = false;
  for( UInt ctuTsAddr = startCtuTsAddr; !isLastCtuOfSliceSegment && ctuTsAddr < numCtusInFrame; ctuTsAddr++)
  {
    const UInt ctuRsAddr = pcPic->getPicSym()->getCtuTsToRsAddrMap(ctuTsAddr);
    const TComTile &currentTile = *(pcPic->getPicSym()->getTComTile(pcPic->getPicSym()->getTileIdxMap(ctuRsAddr)));
    const UInt firstCtuRsAddrOfTile = currentTile.getFirstCtuRsAddr();
    const UInt tileXPosInCtus = firstCtuRsAddrOfTile % frameWidthInCtus;
    const UInt tileYPosInCtus = firstCtuRsAddrOfTile / frameWidthInCtus;
    const UInt ctuXPosInCtus  = ctuRsAddr % frameWidthInCtus;
    const UInt ctuYPosInCtus  = ctuRsAddr / frameWidthInCtus;
    const UInt uiSubStrm=pcPic->getSubstreamForCtuAddr(ctuRsAddr, true, pcSlice)-subStreamOffset;
    TComDataCU* pCtu = pcPic->getCtu( ctuRsAddr );
    pCtu->initCtu( pcPic, ctuRsAddr );

    m_pcEntropyDecoder->setBitstream( ppcSubstreams[uiSubStrm] );

    // set up CABAC contexts' state for this CTU
    if (ctuRsAddr == firstCtuRsAddrOfTile)
    {
      if (ctuTsAddr != startCtuTsAddr) // if it is the first CTU, then the entropy coder has already been reset
      {
        m_pcEntropyDecoder->resetEntropy(pcSlice);
      }
    }
    else if (ctuXPosInCtus == tileXPosInCtus && wavefrontsEnabled)
    {
      // Synchronize cabac probabilities with upper-right CTU if it's available and at the start of a line.
      if (ctuTsAddr != startCtuTsAddr) // if it is the first CTU, then the entropy coder has already been reset
      {
        m_pcEntropyDecoder->resetEntropy(pcSlice);
      }
      TComDataCU *pCtuUp = pCtu->getCtuAbove();
      if ( pCtuUp && ((ctuRsAddr%frameWidthInCtus+1) < frameWidthInCtus)  )
      {
        TComDataCU *pCtuTR = pcPic->getCtu( ctuRsAddr - frameWidthInCtus + 1 );
        if ( pCtu->CUIsFromSameSliceAndTile(pCtuTR) )
        {
          // Top-right is available, so use it.
          pcSbacDecoder->loadContexts( &m_entropyCodingSyncContextState );
        }
      }
    }

#if ENC_DEC_TRACE
    g_bJustDoIt = g_bEncDecTraceEnable;
#endif

#if DECODER_PARTIAL_CONFORMANCE_CHECK != 0
    const UInt numRemainingBitsPriorToCtu=ppcSubstreams[uiSubStrm]->getNumBitsLeft();
#endif

    if ( pcSlice->getSPS()->getUseSAO() )
    {
      SAOBlkParam& saoblkParam = (pcPic->getPicSym()->getSAOBlkParam())[ctuRsAddr];
      Bool bIsSAOSliceEnabled = false;
      Bool sliceEnabled[MAX_NUM_COMPONENT];
      for(Int comp=0; comp < MAX_NUM_COMPONENT; comp++)
      {
        ComponentID compId=ComponentID(comp);
        sliceEnabled[compId] = pcSlice->getSaoEnabledFlag(toChannelType(compId)) && (comp < pcPic->getNumberValidComponents());
        if (sliceEnabled[compId])
        {
          bIsSAOSliceEnabled=true;
        }
        saoblkParam[compId].modeIdc = SAO_MODE_OFF;
      }
      if (bIsSAOSliceEnabled)
      {
        Bool leftMergeAvail = false;
        Bool aboveMergeAvail= false;

        //merge left condition
        Int rx = (ctuRsAddr % frameWidthInCtus);
        if(rx > 0)
        {
          leftMergeAvail = pcPic->getSAOMergeAvailability(ctuRsAddr, ctuRsAddr-1);
        }
        //merge up condition
        Int ry = (ctuRsAddr / frameWidthInCtus);
        if(ry > 0)
        {
          aboveMergeAvail = pcPic->getSAOMergeAvailability(ctuRsAddr, ctuRsAddr-frameWidthInCtus);
        }

        pcSbacDecoder->parseSAOBlkParam( saoblkParam, sliceEnabled, leftMergeAvail, aboveMergeAvail, pcSlice->getSPS()->getBitDepths());
      }
    }

    // hevc_demo
    m_pcCuDecoder->decodeCtu     ( pCtu, isLastCtuOfSliceSegment, cupuOutput );

#if DECODER_PARTIAL_CONFORMANCE_CHECK != 0
    const UInt numRemainingBitsPostCtu=ppcSubstreams[uiSubStrm]->getNumBitsLeft(); // NOTE: Does not account for changes in buffered bits in CABAC decoder, although it's probably good enough.
    if (TDecConformanceCheck::doChecking() && m_pDecConformanceCheck)
    {
      m_pDecConformanceCheck->checkCtuDecoding(numRemainingBitsPriorToCtu-numRemainingBitsPostCtu);
    }
#endif
    // hevc_demo
    predictionOutput << "<" << pCtu->getSlice()->getPOC() << "," << pCtu->getCtuRsAddr() << ">" << " ";
    intraOutput << "<" << pCtu->getSlice()->getPOC() << "," << pCtu->getCtuRsAddr() << ">" << " ";
    motionVectorsOutput << "<" << pCtu->getSlice()->getPOC() << "," << pCtu->getCtuRsAddr() << ">" << " ";

    m_pcCuDecoder->decompressCtu ( pCtu );
    
    // hevc_demo
    WriteCUStats(pCtu, pCtu->getTotalNumPart(), 0, 0, predictionOutput, intraOutput, motionVectorsOutput);
    predictionOutput << endl;
    intraOutput << endl;
    motionVectorsOutput << endl;

#if ENC_DEC_TRACE
    g_bJustDoIt = g_bEncDecTraceDisable;
#endif

    //Store probabilities of second CTU in line into buffer
    if ( ctuXPosInCtus == tileXPosInCtus+1 && wavefrontsEnabled)
    {
      m_entropyCodingSyncContextState.loadContexts( pcSbacDecoder );
    }

    if (isLastCtuOfSliceSegment)
    {
#if DECODER_CHECK_SUBSTREAM_AND_SLICE_TRAILING_BYTES
      pcSbacDecoder->parseRemainingBytes(false);
#endif
      if(!pcSlice->getDependentSliceSegmentFlag())
      {
        pcSlice->setSliceCurEndCtuTsAddr( ctuTsAddr+1 );
      }
      pcSlice->setSliceSegmentCurEndCtuTsAddr( ctuTsAddr+1 );
    }
    else if (  ctuXPosInCtus + 1 == tileXPosInCtus + currentTile.getTileWidthInCtus() &&
             ( ctuYPosInCtus + 1 == tileYPosInCtus + currentTile.getTileHeightInCtus() || wavefrontsEnabled)
            )
    {
      // The sub-stream/stream should be terminated after this CTU.
      // (end of slice-segment, end of tile, end of wavefront-CTU-row)
      UInt binVal;
      pcSbacDecoder->parseTerminatingBit( binVal );
      assert( binVal );
#if DECODER_CHECK_SUBSTREAM_AND_SLICE_TRAILING_BYTES
      pcSbacDecoder->parseRemainingBytes(true);
#endif
    }

  }

  assert(isLastCtuOfSliceSegment == true);


  if( depSliceSegmentsEnabled )
  {
    m_lastSliceSegmentEndContextState.loadContexts( pcSbacDecoder );//ctx end of dep.slice
  }

}

// hevc_demo
Void TDecSlice::WriteCUStats(TComDataCU* pcCU, Int iLength, Int iOffset, UInt iDepth, ofstream& predictionOutput, ofstream& intraOutput, ofstream& motionVectorsOutput)
{
    UChar* puhDepth = pcCU->getDepth();
    SChar* puhPartSize = pcCU->getPartitionSize();

    TComMv rcMV;

    if (puhDepth[iOffset] <= iDepth)
    {
        /// PU number in this leaf CU
        int iNumPart = 0;

        switch (puhPartSize[iOffset])
        {
            case SIZE_2Nx2N:    iNumPart = 1; break;
            case SIZE_2NxN:     iNumPart = 2; break;
            case SIZE_Nx2N:     iNumPart = 2; break;
            case SIZE_NxN:      iNumPart = 4; break;
            case SIZE_2NxnU:    iNumPart = 2; break;
            case SIZE_2NxnD:    iNumPart = 2; break;
            case SIZE_nLx2N:    iNumPart = 2; break;
            case SIZE_nRx2N:    iNumPart = 2; break;
            default:            iNumPart = 0; break; // error
        }

        /// Traverse every PU
        int iPartAddOffset = 0;   ///< PU offset
        for (int i = 0; i < iNumPart; i++)
        {
            switch (puhPartSize[iOffset])
            {
                case SIZE_2NxN:
                    iPartAddOffset = (i == 0) ? 0 : iLength >> 1;
                    break;
                case SIZE_Nx2N:
                    iPartAddOffset = (i == 0) ? 0 : iLength >> 2;
                    break;
                case SIZE_NxN:
                    iPartAddOffset = (iLength >> 2) * i;
                    break;
                case SIZE_2NxnU:
                    iPartAddOffset = (i == 0) ? 0 : iLength >> 3;
                    break;
                case SIZE_2NxnD:
                    iPartAddOffset = (i == 0) ? 0 : (iLength >> 1) + (iLength >> 3);
                    break;
                case SIZE_nLx2N:
                    iPartAddOffset = (i == 0) ? 0 : iLength >> 4;
                    break;
                case SIZE_nRx2N:
                    iPartAddOffset = (i == 0) ? 0 : (iLength >> 2) + (iLength >> 4);
                    break;
                default:
                    assert(puhPartSize[iOffset] == SIZE_2Nx2N);
                    iPartAddOffset = 0;
                break;
            }

            /// Write prediction info
            PredMode ePred = pcCU->getPredictionMode(iOffset + iPartAddOffset);
            Int iPred = ePred;
            if (ePred == MODE_INTER)
            {
                iPred = 1;
            }
            else if (ePred == MODE_INTRA)
            {
                iPred = 2;
            }
            else if (ePred == MODE_NONE)
            {
                iPred = 15;
            }
            predictionOutput << iPred << " ";

            // Write MV info
            int iInterDir = pcCU->getInterDir(iOffset + iPartAddOffset);   ///< Inter direction: 0--Invalid, 1--List 0 only, 2--List 1 only, 3--List 0&1(bi-direction)
            int iRefIdx = -1;
            motionVectorsOutput << iInterDir << " ";
            if (iInterDir == 0)
            {
                // do nothing
            }
            else if (iInterDir == 1)
            {
                rcMV = pcCU->getCUMvField(REF_PIC_LIST_0)->getMv(iOffset + iPartAddOffset);
                iRefIdx = pcCU->getCUMvField(REF_PIC_LIST_0)->getRefIdx(iOffset + iPartAddOffset);
                motionVectorsOutput << pcCU->getSlice()->getRefPOC(REF_PIC_LIST_0, iRefIdx) << " " << rcMV.getHor() << " " << rcMV.getVer() << " ";
            }
            else if (iInterDir == 2)
            {
                rcMV = pcCU->getCUMvField(REF_PIC_LIST_1)->getMv(iOffset + iPartAddOffset);
                iRefIdx = pcCU->getCUMvField(REF_PIC_LIST_1)->getRefIdx(iOffset + iPartAddOffset);
                motionVectorsOutput << pcCU->getSlice()->getRefPOC(REF_PIC_LIST_1, iRefIdx) << " " << rcMV.getHor() << " " << rcMV.getVer() << " ";
            }
            else if (iInterDir == 3)
            {
                rcMV = pcCU->getCUMvField(REF_PIC_LIST_0)->getMv(iOffset + iPartAddOffset);
                iRefIdx = pcCU->getCUMvField(REF_PIC_LIST_0)->getRefIdx(iOffset + iPartAddOffset);
                motionVectorsOutput << pcCU->getSlice()->getRefPOC(REF_PIC_LIST_0, iRefIdx) << " " << rcMV.getHor() << " " << rcMV.getVer() << " ";
                rcMV = pcCU->getCUMvField(REF_PIC_LIST_1)->getMv(iOffset + iPartAddOffset);
                iRefIdx = pcCU->getCUMvField(REF_PIC_LIST_1)->getRefIdx(iOffset + iPartAddOffset);
                motionVectorsOutput << pcCU->getSlice()->getRefPOC(REF_PIC_LIST_1, iRefIdx) << " " << rcMV.getHor() << " " << rcMV.getVer() << " ";
            }

            /// Write Intra info
            Int iLumaIntraDir = pcCU->getIntraDir(CHANNEL_TYPE_LUMA, iOffset + iPartAddOffset);
            Int iChromaIntraDir = pcCU->getIntraDir(CHANNEL_TYPE_CHROMA, iOffset + iPartAddOffset);
            intraOutput << iLumaIntraDir << " " << iChromaIntraDir << " ";

        }
    }
    else
    {
        for (UInt i = 0; i < 4; i++)
        {
            WriteCUStats(pcCU, iLength / 4, iOffset + iLength / 4 * i, iDepth + 1, predictionOutput, intraOutput, motionVectorsOutput);
        }
    }
}
