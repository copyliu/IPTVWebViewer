using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using PlaylistsNET.Content;
using PlaylistsNET.Models;

namespace IPTVWebViewer.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        public class PlayListEntry
        {
            public string name { get; set; }
            public string url { get; set; }
        }

        private readonly IWebHostEnvironment _appEnvironment;

        public PlaylistController(IWebHostEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
        }
        [HttpGet]
        [Route("all")]
        public List<PlayListEntry> GetPlayList()
        {
            var playlist = Path.Combine(_appEnvironment.WebRootPath, "playlist.m3u");
            var parser=PlaylistsNET.Content.PlaylistParserFactory.GetPlaylistParser(PlaylistType.M3U8);
            using var file = new FileStream(playlist, FileMode.Open);
            var playlists=(M3uPlaylist)parser.GetFromStream(file);
            return playlists.PlaylistEntries.Select((p,i) => new PlayListEntry()
            {
                name = p.Title + playlists.Comments.Skip(i).FirstOrDefault(),
                url = p.Path
            }).Where(p => p.url.StartsWith("rtp://")).ToList();

        }
    }
}
