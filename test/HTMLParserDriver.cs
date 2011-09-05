using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SimpleHtmlParser
{
    public class HtmlParserDriver : SimpleHTMLParserBase
    {
        private Dictionary<string, Action<string, ReadOnlyCollection<HtmlAttribute>>> startTagHandlers = new Dictionary<string, Action<string, ReadOnlyCollection<HtmlAttribute>>>();
        private Dictionary<string, Action<string, string>> endTagHandlers = new Dictionary<string, Action<string, string>>();
        private Dictionary<string, Action<string, ReadOnlyCollection<HtmlAttribute>>> startEndTagHandlers = new Dictionary<string, Action<string, ReadOnlyCollection<HtmlAttribute>>>();

        public void RegisterStartTagHandler(string tagName, Action<string, ReadOnlyCollection<HtmlAttribute>> handler)
        {
            if (!this.startTagHandlers.ContainsKey(tagName))
            {
                this.startTagHandlers.Add(tagName, handler);
            }
            else
            {
                this.startTagHandlers[tagName] += handler;
            }
        }

        public void RegisterEndTagHandler(string tagName, Action<string, string> handler)
        {
            this.endTagHandlers.Add(tagName, handler);
        }

        public void RegisterStartEndTagHandler(string tagName, Action<string, ReadOnlyCollection<HtmlAttribute>> handler)
        {
            this.startEndTagHandlers.Add(tagName, handler);
        }

        protected override void HandleStartTag(string tagName, ReadOnlyCollection<HtmlAttribute> attributes)
        {
            if (this.startTagHandlers.ContainsKey(tagName))
            {
                this.startTagHandlers[tagName](tagName, attributes);
            }
        }

        protected override void HandleEndTag(string tagName, string content)
        {
            if (this.endTagHandlers.ContainsKey(tagName))
            {
                this.endTagHandlers[tagName](tagName, content);
            }
        }

        protected override void HandleStartEndTag(string tagName, ReadOnlyCollection<HtmlAttribute> attributes)
        {
            if (this.startEndTagHandlers.ContainsKey(tagName))
            {
                this.startEndTagHandlers[tagName](tagName, attributes);
            }
        }
    }
}
