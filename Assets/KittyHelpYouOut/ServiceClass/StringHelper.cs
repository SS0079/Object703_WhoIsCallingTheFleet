using System;
using System.Text;

namespace KittyHelpYouOut.ServiceClass
{
    public static class StringHelper
    {
        private const char SPACE = ' ';
        private const char DASH = '_';
        public enum SpellCase
        {
            Camel,
            Pascal,
            None
        }
        
        public static string ToPascalCaseWithSpaces(this string input)  
        {  
            if (string.IsNullOrEmpty(input))  
            {  
                return input;  
            }  
      
            StringBuilder sb = new StringBuilder();  
      
            for (int i = 0; i < input.Length; i++)  
            {  
                //iterate through string
                //if this char is a letter, see if the previous char is - ,if not , add a space before this char, else, do nothing
                //if this char is not a letter, see if this char is _ , if so, ignore this char
                // if this char is ther first char in the string, convert it to upper class, if not ,do nothing
                
                char c = input[i];

                if (c.IsLetter())
                {
                    if (i==0)
                    {
                        sb.Append(c.ToUpper());
                        continue;
                    }
                    if (c.IsUpper() && !input[i-1].IsUpper())
                    {
                        sb.Append(SPACE);
                    }
                    sb.Append(c);
                }
                else
                {
                    if (c!=DASH)
                    {
                        sb.Append(c);
                    }
                }
            }  
            return sb.ToString();  
        } 

        public static char ToUpper(this char c) => char.ToUpper(c);
        public static char ToLower(this char c) => char.ToLower(c);
        public static bool IsUpper(this char c) => char.IsUpper(c);
        public static bool IsLower(this char c) => char.IsLower(c);
        public static bool IsLetter(this char c) => char.IsLetter(c);
    }
}