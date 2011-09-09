namespace SimpleHTMLParserSharp
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class HTMLAttribute
    {
        private string _name;
        private string _value;

        /// <summary>
        /// parse the string into name and value pair.
        /// </summary>
        /// <param name="att_str">the string is in the format: name=value, and it contains only one paire</param>
        public HTMLAttribute(string att_str)
        {
            if (string.IsNullOrEmpty(att_str))
            {
                throw new ArgumentNullException("att_str");
            }

            int eq_index = att_str.IndexOf('=');
            if (eq_index == -1)
            {
                throw new ArgumentOutOfRangeException("att_str", "attribute string is in bad format.");
            }
            else
            {
                this._name = att_str.Substring(0, eq_index);
                this._value = att_str.Substring(eq_index + 1, att_str.Length - eq_index - 2).Trim('"', '\'');
            }
        }

        public string Name
        {
            get { return this._name; }
        }

        public string Value
        {
            get { return this._value; }
        }

        public override string ToString()
        {
            return string.Format("{0}=\"{1}\"", this._name, this._value);
        }
    }

    public class SimpleHTMLAttributeParser
    {
        static public readonly ReadOnlyCollection<HTMLAttribute> EmptyAttributeCollection = new ReadOnlyCollection<HTMLAttribute>(new List<HTMLAttribute>());

        private List<HTMLAttribute> _attributes = new List<HTMLAttribute>();

        public SimpleHTMLAttributeParser(string str)
        {
            str = str.Trim();
            if (!string.IsNullOrEmpty(str))
            {
                this.Process(str);
            }
        }

        public ReadOnlyCollection<HTMLAttribute> Attributes
        {
            get
            {
                return new ReadOnlyCollection<HTMLAttribute>(this._attributes);
            }
        }

        /// <summary>
        /// parse the string and create a list of HTMLAttribute object
        /// </summary>
        /// <param name="att_str">a string of attributes in the format: name=value key=value</param>
        private void Process(string att_str)
        {
            string[] arr = att_str.Trim().Split(' ');
            arr = (from a in arr
                   where !string.IsNullOrEmpty(a)
                   select a.Trim()).ToArray();

            for (int i = 0; i < arr.Length; i++)
            {
                if (i == arr.Length - 1)
                {
                    // add the last item
                    this._attributes.Add(new HTMLAttribute(arr[i]));
                }
                else
                {
                    if (arr[i].IndexOf('=') >= 0
                        && arr[i + 1].IndexOf('=') == -1)
                    {
                        // the current item contains '=' but the next item does not,
                        // this two items should be catenated.
                        // the new value is stored in the next item
                        arr[i + 1] = string.Format("{0} {1}", arr[i], arr[i + 1]);
                    }
                    else if (arr[i].IndexOf('=') == -1
                        && arr[i + 1].IndexOf('=') == -1)
                    {
                        // both the current item and the next item does not contain the '='.
                        // this two items should be catenated.
                        // the new value is stored in the next item
                        arr[i + 1] = string.Format("{0} {1}", arr[i], arr[i + 1]);
                    }
                    else
                    {
                        // we have a new attribute
                        this._attributes.Add(new HTMLAttribute(arr[i]));
                    }
                }
            }
        }
    }
}
