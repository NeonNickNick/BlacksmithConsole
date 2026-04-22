namespace Blacksmith.FrontendBackendInterface.Web
{
    public record WebModeOption(int Id, string Name);

    public record WebActionRecord(string SkillName, int Param);

    public record WebResourceRecord(string Name, float Common, float Gold, float Total);

    public record WebDefenseRecord(string Name, int Power);

    public record WebActorSnapshot(
        string Name,
        string Profession,
        int HP,
        int MaxHP,
        IReadOnlyList<WebResourceRecord> Resources,
        IReadOnlyList<WebDefenseRecord> Defenses,
        IReadOnlyList<string> AvailableSkills,
        IReadOnlyList<string> ActivePackages);

    public record WebTurnRecord(int Index, WebActionRecord Player, WebActionRecord Enemy, string Result);

    public record WebGameSnapshot(
        bool Started,
        string ModeName,
        bool ManualMode,
        string Result,
        int TurnCount,
        WebActorSnapshot? Player,
        WebActorSnapshot? Enemy,
        IReadOnlyList<WebTurnRecord> Turns);

    public record WebCommandResult(bool Ok, string Message, WebGameSnapshot Snapshot);
}
