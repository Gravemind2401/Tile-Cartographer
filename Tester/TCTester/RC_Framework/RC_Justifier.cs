// Justifier.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;



namespace RC_Framework
{
    class Justifier
    {
        //public static String[] inp;
        //public static String[] outp;
        public static StringList tmp;
        //public static int curLine;
        //public static int curChar;
        //public static String curWord;
        public static String curOutLine;
        public static char[] delimiterChars = { ' ', '\t' };

        public static String[] justify(String[] inputStr, int len)
        {
            tmp = new StringList();
            curOutLine = "";

            for (int i = 0; i < inputStr.Length; i++)
            {
                string text = inputStr[i].Trim();
                string[] words = text.Split(delimiterChars);

                foreach (string ss in words)
                {

                    string s = ss.Trim();

                    if (s == "<p>")
                {
                    tmp.Add(curOutLine);
                    curOutLine = "";
                }
                else if (curOutLine.Length + s.Length > len)
                    {
                    tmp.Add(curOutLine);
                    curOutLine = s + " "; ;
                    }
                else
                    {
                    curOutLine =curOutLine +s+" ";
                    }
                }
            }
            tmp.Add(curOutLine.Trim());
            String[] rc = new String[tmp.Count];
            for (int i = 0; i < tmp.Count; i++)
            {
                rc[i] = tmp[i];
            }
            return rc;
        }

    }
}
// Justifier.cs