namespace Skybot.Web.Wiki
{
    public class WikiPage
    {
        public string HTML { get; }
        public string Script { get; }

        public WikiPage(string html, string script)
        {
            HTML = html;
            Script = script;
        }

        public void HtmlToFile(string file)
        {
            System.IO.File.WriteAllText(file, HTML);
        }

        public void ScriptToFile(string file)
        {
            System.IO.File.WriteAllText(file, Script);
        }
    }
}
