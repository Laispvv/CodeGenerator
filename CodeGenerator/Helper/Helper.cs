using System.Globalization;
using System.Text.RegularExpressions;

namespace CodeGenerator.Helper
{
    public static class Helper
    {
        public static string ToCamelCase(string text)
        {
            TextInfo txtInfo = new CultureInfo("pt-br", false).TextInfo;
            return txtInfo.ToTitleCase(text).Replace("_", string.Empty);
        }
        public static string ToCamelCaseLoweFirst(string text)
        {
            TextInfo txtInfo = new CultureInfo("pt-br", false).TextInfo;
            text = txtInfo.ToTitleCase(text).Replace("_", string.Empty);
            return Char.ToLowerInvariant(text[0]) + text.Substring(1);
        }
        public static void CreateFiles(string templateFilePath, string nameSpace, string className)
        {
            StreamReader rd = new StreamReader(templateFilePath);
            string strContents = rd.ReadToEnd();
            rd.Close();

            Regex regexText = new Regex("#NAMESPACE#");
            strContents = regexText.Replace(strContents, ToCamelCase(nameSpace));
            regexText = new Regex("#CLASSNAME#");
            strContents = regexText.Replace(strContents, ToCamelCase(className));
            regexText = new Regex("#VARCLASSNAME#");
            strContents = regexText.Replace(strContents, ToCamelCaseLoweFirst(className));

            string pathToSave = @"C:\Users\Lais\source\repos\CodeGenerator\CodeGenerator\DownloadFiles\" +
                                Path.GetFileNameWithoutExtension(templateFilePath).Replace(".", @"\");

            string fileName = className + "." + "cs";

            var path = Path.Combine(pathToSave, fileName);

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(strContents);
                    sw.Close();
                }
            }
        }
        public static void DeleteOlderFiles(List<string> filesNameToGenerate, string downloadFilePath)
        {
            foreach (var templateFilePath in filesNameToGenerate)
            {
                var pathFileToDelete = downloadFilePath +
                                         @"\" + Path.GetFileNameWithoutExtension(templateFilePath).Replace(".", @"\");
                var directoryInfo = new DirectoryInfo(pathFileToDelete);
                foreach (FileInfo file in directoryInfo.GetFiles())
                    file.Delete();
            }
        }
    }
}
