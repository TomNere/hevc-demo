using Rasyidf.Localization;
using System.IO;

namespace HEVCDemo.Helpers
{
    public static class MarkdownLoader
    {
        public static string GetInfoText(string textKey)
        {
            string currentLanguage = LocalizationService.Current.LanguagePack.EnglishName;
            string path = $@".\Assets\InfoTexts\{currentLanguage}\{textKey}.md";
            return File.ReadAllText(path);
        }
    }
}
