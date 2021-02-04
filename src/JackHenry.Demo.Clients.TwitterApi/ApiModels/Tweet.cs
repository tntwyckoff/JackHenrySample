using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackHenry.Demo.Clients.TwitterApi.ApiModels
{
    public class Tweet
    {
        public string id { get; set; }
        public string text { get; set; }
        public string author_id { get; set; }
        public string created_at { get; set; }
        public Entities entities { get; set; }
        public bool HasHashTag { get { return null != entities && null != entities.hashtags && 0 < entities.hashtags.Count; } }
        public bool HasUrl { get { return null != entities && null != entities.hashtags && 0 < entities.urls.Count; } }
        public bool HasPhotoUrl { get { return HasUrl && entities.urls.Any(u => -1 < u.display_url.IndexOf("pic.twitter.com") || -1 < u.display_url.IndexOf("http://instagram.com/")); } }

        public DateTime CreatedDateAsDate 
        {
            get
            {
                DateTime result;

                if (DateTime.TryParse(created_at, out result))
                    return result;

                return DateTime.Now;
            }
        }
    }
}
