using System.Collections.Generic;

namespace rail
{
	public interface IRailVoiceChannel : IRailComponent
	{
		RailVoiceChannelID GetVoiceChannelID();

		RailResult GetVoiceChannelName(out string name);

		RailResult JoinVoiceChannel();

		RailResult LeaveVoiceChannel();

		EnumRailVoiceChannelSpeakerState GetSpeakerState();

		RailResult MuteSpeaker();

		RailResult ResumeSpeaker();

		RailResult GetUsers(List<RailID> user_list);

		RailResult AddUsers(List<RailID> user_list);

		RailResult RemoveUsers(List<RailID> user_list);

		RailResult CloseChannel();
	}
}
