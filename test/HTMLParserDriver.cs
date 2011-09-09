using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SimpleHTMLParserSharp.Text
{
    public class HTMLParserDriver : SimpleHTMLParserBase
    {
        private Dictionary<string, Action<string, ReadOnlyCollection<HTMLAttribute>>> startTagHandlers = new Dictionary<string, Action<string, ReadOnlyCollection<HTMLAttribute>>>();
        private Dictionary<string, Action<string, string>> endTagHandlers = new Dictionary<string, Action<string, string>>();
        private Dictionary<string, Action<string, ReadOnlyCollection<HTMLAttribute>>> startEndTagHandlers = new Dictionary<string, Action<string, ReadOnlyCollection<HTMLAttribute>>>();

        public void RegisterStartTagHandler(string tagName, Action<string, ReadOnlyCollection<HTMLAttribute>> handler)
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

        public void RegisterStartEndTagHandler(string tagName, Action<string, ReadOnlyCollection<HTMLAttribute>> handler)
        {
            this.startEndTagHandlers.Add(tagName, handler);
        }

        protected override void HandleStartTag(string tagName, ReadOnlyCollection<HTMLAttribute> attributes)
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

        protected override void HandleStartEndTag(string tagName, ReadOnlyCollection<HTMLAttribute> attributes)
        {
            if (this.startEndTagHandlers.ContainsKey(tagName))
            {
                this.startEndTagHandlers[tagName](tagName, attributes);
            }
        }
    }
}
