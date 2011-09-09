namespace SimpleHtmlParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Diagnostics;
    using System.IO;

    public static class Utility
    {
        /// <summary>
        /// default html tag filter; executed in order
        /// </summary>
        private static Regex[] re_tag_filter = new Regex[]
        {
            new Regex("<script.*?</script>", RegexOptions.Singleline | RegexOptions.Compiled), // script
            new Regex("<style.*?</style>", RegexOptions.Singleline | RegexOptions.Compiled), // style
            new Regex("<!.*?>", RegexOptions.Singleline | RegexOptions.Compiled), // pi
            new Regex("<.*?>", RegexOptions.Singleline | RegexOptions.Compiled) // tag
        }

        private static Regex[] re_noise_filter = new Regex[]
        {
            new Regex("[0-9]+", RegexOptions.Singleline|RegexOptions.Compiled),
            new Regex(" ?[~!@#\\$%\\^&\\*\\(\\)_\\+{}|\\[\\]\\:\";'\\<\\>\\?,\\./\\-=]+ ?", RegexOptions.Singleline | RegexOptions.Compiled),
            new Regex("[©®«»]+", RegexOptions.Singleline | RegexOptions.Compiled)
        }

        /// <summary>
        /// a helper method to retrieve the normalized text in the given html source
        /// </summary>
        /// <param name="html_src">html source</param>
        /// <returns>normalized text in the html source</returns>
        public static string GetNormalText(string html_src)
        {
            foreach (var re in re_tag_filter)
            {
                html_src = re.Replace(html_src, " ");
            }

            html_src = HttpUtility.HtmlDecode(html_src);

            return html_src;
        }

        public static string NormalizeSpace(string str)
        {
            str = str.Trim();
            Regex reSpace = new Regex("[ \n\t]+", RegexOptions.Singleline | RegexOptions.Compiled);
            StringBuilder sb = new StringBuilder();
            string l = null;

            using (StringReader sr = new StringReader(str))
            {
                while ((l = sr.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(l = l.Trim()))
                    {
                        sb.AppendLine(reSpace.Replace(l, " "));
                    }
                }
            }

            return sb.ToString().Trim();
        }

        public static string CleanNoise(string str)
        {
            foreach (Regex r in re_noise_filter)
            {
                str = r.Replace(str, " ");
            }

            return str;
        }

        public static Dictionary<string, int> PopulateWordDict(string str)
        {
            string[] arr = str.Split(' ');
            Dictionary<string, int> dict = new Dictionary<string, int>();

            var d = from w in arr
                    group w by w into g
                    // orderby gw.Count() descending
                    select new { Word = g.Key, Count = g.Count() };

            foreach (var e in d)
            {
                dict.Add(e.Word, e.Count);
            }

            return dict;
        }
    }
}
