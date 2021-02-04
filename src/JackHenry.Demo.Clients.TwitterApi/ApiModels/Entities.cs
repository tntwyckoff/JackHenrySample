using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackHenry.Demo.Clients.TwitterApi.ApiModels
{
    public class Entities
    {
        public List<Hashtag> hashtags { get; set; } = new List<Hashtag>();
        public List<Url> urls { get; set; } = new List<Url>();
    }
}
