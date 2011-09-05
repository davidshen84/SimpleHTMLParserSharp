namespace SimpleHtmlParser
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Web;

    public abstract class SimpleHTMLParserBase
    {
        private static Regex re_good_tag_name = new Regex("^[a-z0-9:_]*$", RegexOptions.Singleline | RegexOptions.Compiled);
        private bool inScript = false;

        public void Process(string html_src)
        {
            bool process = true;
            int last_gt_index = 0;
            Stack<int> last_gt_indices = new Stack<int>();
            Stack<string> tag_name_context = new Stack<string>();

            if (string.IsNullOrEmpty(html_src))
            {
                throw new ArgumentNullException("html_src");
            }

            while (process)
            {
                // the index of '<'
                int lt_index = html_src.IndexOf('<', last_gt_index);

                if (lt_index >= 0)
                {
                    // the index of '>'
                    int gt_index = html_src.IndexOf('>', lt_index);

                    if (gt_index > -1)
                    {
                        // remember the last index of '>'
                        last_gt_index = gt_index;

                        // extract the tag
                        string str = html_src.Substring(lt_index, gt_index - lt_index + 1);

#if DEBUG
                        Debug.Assert(str.Length > 0, "tag name is empty");
#endif

                        switch (str[1])
                        {
                            case '/':
                                this.ProcessEndTag(str, html_src, last_gt_indices, tag_name_context, lt_index);
                                break;
                            case '!':
                                this.ProcessPITag(str);
                                break;
                            default:
                                this.ProcessStartTag(last_gt_indices, tag_name_context, gt_index, str);
                                break;
                        }
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("html_src", "the '<' does not have a matching '>'.");
                    }
                }
                else
                {
#if TRACE
                    Trace.TraceInformation("end processing.");
#endif

#if DEBUG
                    Debug.Assert(tag_name_context.Count == 0, "process did not end gracefully.");
#endif

                    process = false;
                }
            }
        }

        #region Abastrct Methods
        protected virtual void HandleStartTag(string tagName, ReadOnlyCollection<HtmlAttribute> attributes) { }

        protected virtual void HandleStartEndTag(string tagName, ReadOnlyCollection<HtmlAttribute> attributes) { }

        protected virtual void HandleEndTag(string tagName, string content) { }

        protected virtual void HandlePITag(string content) { }
        #endregion

        private void ProcessStartTag(Stack<int> last_gt_indices, Stack<string> tag_name_context, int gt_index, string str)
        {
            // process the start tag
            int index_of_sp = str.IndexOf(' ');

            if (index_of_sp > 0)
            {
                // extract the tag name
                string tag_name = str.Substring(1, index_of_sp - 1).Trim();
                if (!re_good_tag_name.IsMatch(tag_name) || inScript)
                {
                    // bad match, advance to the next '>'
                    return;
                }
                else
                {
                    if (str[str.Length - 2] == '/')
                    {
                        // process the start & end tag
                        string attr_str = str.Substring(index_of_sp + 1, str.Length - index_of_sp - 3);
                        var attr = string.IsNullOrEmpty(attr_str) ? null : new SimpleHtmlAttributeParser(attr_str).Attributes;


                        this.HandleStartEndTag(tag_name, attr);
                    }
                    else
                    {
                        // process the start tag

                        // the process context advance to the next level
                        last_gt_indices.Push(gt_index);
                        tag_name_context.Push(tag_name);
                        if (tag_name == "script")
                        {
                            inScript = true;
                        }

                        // extrace the attributes
                        string attr_str = str.Substring(index_of_sp + 1, str.Length - index_of_sp - 2);
                        var attr = string.IsNullOrEmpty(attr_str) ? null : new SimpleHtmlAttributeParser(attr_str).Attributes;

                        // call the handler
                        this.HandleStartTag(tag_name, attr);
                    }
                }
            }
            else
            {
                // the tag does not have attributes

                // extrace the tag name
                string tag_name = str.Substring(1, str.Length - 2).Trim();
                if (!re_good_tag_name.IsMatch(tag_name) || inScript)
                {
                    // bad match, advance to the next '>'
                    return;
                }
                else
                {
                    if (str[str.Length - 2] == '/')
                    {
                        // process the start & end tag
                        this.HandleStartEndTag(tag_name, SimpleHtmlAttributeParser.EmptyAttributeCollection);
                    }
                    else
                    {
                        // process the start tag
                        last_gt_indices.Push(gt_index);

                        tag_name_context.Push(tag_name);
                        if (tag_name == "script")
                        {
                            this.inScript = true;
                        }

                        this.HandleStartTag(tag_name, SimpleHtmlAttributeParser.EmptyAttributeCollection);
                    }
                }
            }
        }

        private void ProcessEndTag(string end_tag, string html_src, Stack<int> last_gt_indices, Stack<string> tag_name_context, int lt_index)
        {
            // process the end tag

            // get the index of '>' which match the current index of '<', and extrace the content.
            int prev_index_of_gt = last_gt_indices.Pop();
            string content = html_src.Substring(prev_index_of_gt + 1, lt_index - prev_index_of_gt - 1);

            // extract the tag name
            string tag_name = end_tag.Substring(2, end_tag.Length - 3).Trim();

            if (!tag_name_context.Contains(tag_name))
            {
                // we found an extraneous close tag
                // put the previous '>' index back to the stack
                last_gt_indices.Push(prev_index_of_gt);
                return;
            }

            while (tag_name_context.Count > 0 && tag_name != tag_name_context.Peek())
            {
                // tag mismatch. assuming a matching tag is insertted.
                this.HandleEndTag(tag_name_context.Peek(), content);

                if (tag_name_context.Pop() == "script")
                {
                    this.inScript = false;
                }

                // revise the index, and extract the content
                prev_index_of_gt = last_gt_indices.Pop();
                content = html_src.Substring(prev_index_of_gt + 1, lt_index - prev_index_of_gt - 1);
            }

            // tag end normally
            this.HandleEndTag(tag_name, content);

            if (tag_name_context.Pop() == "script")
            {
                this.inScript = false;
            }
        }

        private void ProcessPITag(string str)
        {
            // process the PI
            this.HandlePITag(str.Substring(1, str.Length - 2));
        }
    }
}
