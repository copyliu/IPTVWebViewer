using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using Microsoft.Net.Http.Headers;

namespace IPTVWebViewer.Controller
{
    [Route("api/stream")]
    [ApiController]
    public class StreamController : ControllerBase
    {
        [Route("{rtpurl}")]
        [HttpGet]
        public async Task RtpStream(string rtpurl, CancellationToken cancellationToken)
        {
            this.Response.StatusCode = 200;
            this.Response.Headers.Add(HeaderNames.ContentDisposition, $"attachment; filename=\"stream\"");
            this.Response.Headers.Add(HeaderNames.ContentType, "application/octet-stream");
            var pipe = new StreamPipeSink(this.Response.Body);
            var arg=FFMpegCore.FFMpegArguments.FromFileInput($"rtp://{rtpurl}", false).OutputToPipe(pipe, options =>

            {
                options.ForceFormat("mpegts");
                options.WithAudioCodec("aac");
                options.WithVideoCodec("copy");
                options.WithFastStart();
                options.UsingMultithreading(true);
                options.WithCustomArgument("-analyzeduration 500000");
                options.WithCustomArgument("-fflags nobuffer");
                options.WithCustomArgument("-flags low_delay");
                options.WithCustomArgument("-max_delay 250000");
                options.WithCustomArgument("-max_interleave_delta 1"); // see https://github.com/l3tnun/EPGStation/issues/433

            });
            arg.CancellableThrough(out var cancel);
            cancellationToken.Register(cancel);
            await arg.ProcessAsynchronously();

        }
    }
}
