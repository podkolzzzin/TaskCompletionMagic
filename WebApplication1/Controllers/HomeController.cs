using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class MessageRequest
{
    public string Msg { get; set; }
}

public class WaitMessageRequest
{
    public int Id { get; set; }
}

public class HomeController : Controller
{
    private static readonly List<string> _messages = new ();
    private static TaskCompletionSource _source = new();
    
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public void SendMessage([FromBody]MessageRequest m)
    {
        lock (_messages)
        {            
            _messages.Add(m.Msg);
            _source.SetResult();
            _source = new();
        }
    }

    public async Task<IEnumerable<string>> WaitMessage([FromBody]WaitMessageRequest request)
    {
        IEnumerable<string>? GetMessages(int msgId)
        {
            lock (_messages)
            {
                if (_messages.Count > msgId)
                    return _messages.Skip(msgId);
            }

            return null;
        }

        var m = GetMessages(request.Id);
        if (m != null)
            return m;

        var t = _source.Task;
        
        m = GetMessages(request.Id);
        if (m != null)
            return m;

        await t.WaitAsync(TimeSpan.FromMinutes(2));
        
        m = GetMessages(request.Id);
        if (m != null)
            return m;
        
        return ArraySegment<string>.Empty;
    }
}