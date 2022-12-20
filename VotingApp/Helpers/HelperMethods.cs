using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;
using VotingApp.Data;
using VotingApp.Models;
using Humanizer;

namespace VotingApp.Helpers
{
    public class HelperMethods
    {
        private readonly ApplicationDbContext _context;


        public HelperMethods(ApplicationDbContext context)
        {
            _context = context;
        }  
 
        // generates a slug using idea.Title
        public string GenerateSlug(Idea idea)
        {
            if (idea.Title == null) return "";
            const int maxlen = 80;
            var len = idea.Title.Length;
            var prevdash = false;
            var sb = new StringBuilder(len);
            char c;
            for (int i = 0; i < len; i++)
            {
                c = idea.Title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lowercase
                    sb.Append((char)(c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
                        c == '\\' || c == '-' || c == '_' || c == '=')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else if (c == '#')
                {
                    if (i > 0)
                        if (idea.Title[i - 1] == 'C' || idea.Title[i - 1] == 'F')
                            sb.Append("-sharp");
                }
                else if (c == '+')
                {
                    sb.Append("-plus");
                }
                else if ((int)c >= 128)
                {
                    int prevlen = sb.Length;
                    sb.Append(RemapInternationalCharToAscii(c));
                    if (prevlen != sb.Length) prevdash = false;
                }
                if (sb.Length == maxlen) break;
            }
            if (prevdash)
                return sb.ToString().Substring(0, sb.Length - 1);
            else
                return sb.ToString();
        }

        // converts internation characters to ascii
        // used by GenerateSlug() method
        private string RemapInternationalCharToAscii(char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(s))
            {
                return "a";
            }
            else if ("èéêëę".Contains(s))
            {
                return "e";
            }
            else if ("ìíîïı".Contains(s))
            {
                return "i";
            }
            else if ("òóôõöøőð".Contains(s))
            {
                return "o";
            }
            else if ("ùúûüŭů".Contains(s))
            {
                return "u";
            }
            else if ("çćčĉ".Contains(s))
            {
                return "c";
            }
            else if ("żźž".Contains(s))
            {
                return "z";
            }
            else if ("śşšŝ".Contains(s))
            {
                return "s";
            }
            else if ("ñń".Contains(s))
            {
                return "n";
            }
            else if ("ýÿ".Contains(s))
            {
                return "y";
            }
            else if ("ğĝ".Contains(s))
            {
                return "g";
            }
            else if (c == 'ř')
            {
                return "r";
            }
            else if (c == 'ł')
            {
                return "l";
            }
            else if (c == 'đ')
            {
                return "d";
            }
            else if (c == 'ß')
            {
                return "ss";
            }
            else if (c == 'Þ')
            {
                return "th";
            }
            else if (c == 'ĥ')
            {
                return "h";
            }
            else if (c == 'ĵ')
            {
                return "j";
            }
            else
            {
                return "";
            }
        }

        // check if the slug name exist in the database
        public bool SlugExist(string slug)
        {
            return _context.Idea.Any(Post => Post.Slug == slug);
        }

        // converts given timespan into human readable
        public string ConvertTimeSpan(Idea idea)
        {
            double timestamp = idea.CreatedDate.ToOADate();
            var timeSpan = DateTime.FromOADate(timestamp).AddMicroseconds(-timestamp).Humanize();

            return timeSpan;
        }


    }
}
