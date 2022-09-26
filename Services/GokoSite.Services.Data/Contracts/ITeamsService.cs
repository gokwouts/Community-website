namespace GokoSite.Services.Data
{
    using System.Collections.Generic;

    using RiotSharp.Endpoints.MatchEndpoint;

    public interface ITeamsService
    {
        long GetTotalGoldByPlayers(List<ParticipantIdentity> participantIdentities, List<Participant> participants, int teamId);
    }
}
