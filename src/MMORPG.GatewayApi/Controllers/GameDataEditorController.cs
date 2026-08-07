using Microsoft.AspNetCore.Mvc;
using MMORPG.Domain.Interfaces;

namespace MMORPG.GatewayApi.Controllers;

[ApiController]
[Route("api/editor")]
public class GameDataEditorController : ControllerBase
{
    private readonly IGameDataEditorService _editorService;

    public GameDataEditorController(IGameDataEditorService editorService)
    {
        _editorService = editorService;
    }

    [HttpGet("items")]
    public IActionResult GetItems() => Ok(_editorService.GetAllItems());

    [HttpPost("items")]
    public IActionResult SaveItem([FromBody] DynamicItemData item)
    {
        _editorService.SaveItem(item);
        return Ok(new { message = $"Item '{item.Name}' saved successfully!", item });
    }

    [HttpDelete("items/{itemId}")]
    public IActionResult DeleteItem(string itemId)
    {
        bool success = _editorService.DeleteItem(itemId);
        return success ? Ok(new { message = $"Item '{itemId}' deleted." }) : NotFound(new { error = "Item not found." });
    }

    [HttpGet("monsters")]
    public IActionResult GetMonsters() => Ok(_editorService.GetAllMonsters());

    [HttpPost("monsters")]
    public IActionResult SaveMonster([FromBody] DynamicMonsterData monster)
    {
        _editorService.SaveMonster(monster);
        return Ok(new { message = $"Monster '{monster.Name}' saved successfully!", monster });
    }

    [HttpDelete("monsters/{monsterId}")]
    public IActionResult DeleteMonster(string monsterId)
    {
        bool success = _editorService.DeleteMonster(monsterId);
        return success ? Ok(new { message = $"Monster '{monsterId}' deleted." }) : NotFound(new { error = "Monster not found." });
    }

    [HttpGet("npcs")]
    public IActionResult GetNpcs() => Ok(_editorService.GetAllNpcs());

    [HttpPost("npcs")]
    public IActionResult SaveNpc([FromBody] DynamicNpcData npc)
    {
        _editorService.SaveNpc(npc);
        return Ok(new { message = $"NPC '{npc.Name}' saved successfully!", npc });
    }

    [HttpDelete("npcs/{npcId}")]
    public IActionResult DeleteNpc(string npcId)
    {
        bool success = _editorService.DeleteNpc(npcId);
        return success ? Ok(new { message = $"NPC '{npcId}' deleted." }) : NotFound(new { error = "NPC not found." });
    }

    [HttpGet("maps")]
    public IActionResult GetMaps() => Ok(_editorService.GetAllMaps());

    [HttpPost("maps")]
    public IActionResult SaveMap([FromBody] DynamicMapData map)
    {
        _editorService.SaveMap(map);
        return Ok(new { message = $"Map '{map.Name}' (Zone #{map.ZoneId}) saved successfully!", map });
    }

    [HttpDelete("maps/{zoneId}")]
    public IActionResult DeleteMap(int zoneId)
    {
        bool success = _editorService.DeleteMap(zoneId);
        return success ? Ok(new { message = $"Map (Zone #{zoneId}) deleted." }) : NotFound(new { error = "Map not found." });
    }

    [HttpGet("classes")]
    public IActionResult GetClasses() => Ok(_editorService.GetAllClasses());

    [HttpPost("classes")]
    public IActionResult SaveClass([FromBody] DynamicClassDefinition classDef)
    {
        _editorService.SaveClass(classDef);
        return Ok(new { message = $"Class '{classDef.ClassName}' saved successfully!", classDef });
    }

    [HttpGet("skills")]
    public IActionResult GetSkills() => Ok(_editorService.GetAllSkills());

    [HttpPost("skills")]
    public IActionResult SaveSkill([FromBody] DynamicSkillDefinition skillDef)
    {
        _editorService.SaveSkill(skillDef);
        return Ok(new { message = $"Skill '{skillDef.Name}' saved successfully!", skillDef });
    }
}
