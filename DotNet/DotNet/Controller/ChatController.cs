

using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Packaging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.IO;
using DotNetLLM.Service;
using DotNetLLM.DTO;

namespace DotNetLLM.Controller;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatService _service;
    public ChatController(ChatService service)
    {
        _service = service;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request)
    {
        var response = await _service.Chat(request);
        return Ok(response);
    }
}

