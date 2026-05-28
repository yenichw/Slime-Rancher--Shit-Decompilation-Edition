using System.Collections.Generic;

namespace MonomiPark.SlimeRancher.DataModel
{
	public class MailModel
	{
		public interface Participant
		{
			void InitModel(MailModel model);

			void SetModel(MailModel model);

			void OnMailListChanged();
		}

		public List<MailDirector.Mail> allMail = new List<MailDirector.Mail>();

		public Dictionary<MailDirector.Mail, MailDirector.Mail> allMailDict = new Dictionary<MailDirector.Mail, MailDirector.Mail>();

		public bool hasPartnerMail;

		public bool hasSecretStyleMail;

		public bool hasNewMail;

		private Participant participant;

		public void SetParticipant(Participant participant)
		{
			this.participant = participant;
		}

		public void Init()
		{
			if (participant != null)
			{
				participant.InitModel(this);
			}
		}

		public void NotifyParticipants()
		{
			if (participant != null)
			{
				participant.SetModel(this);
			}
		}

		public void Reset()
		{
			allMail.Clear();
			allMailDict.Clear();
			hasPartnerMail = false;
			hasSecretStyleMail = false;
			hasNewMail = false;
		}

		public void AddMail(MailDirector.Mail mail)
		{
			allMail.Add(mail);
			allMailDict[mail] = mail;
			MailListChanged();
		}

		public void RefreshNewMail()
		{
			hasPartnerMail = false;
			hasSecretStyleMail = false;
			foreach (MailDirector.Mail item in allMail)
			{
				allMailDict[item] = item;
				if (item.key == "partner_rewards")
				{
					hasPartnerMail = true;
				}
				else if (item.key == "secret_styles")
				{
					hasSecretStyleMail = true;
				}
			}
			hasNewMail = false;
			foreach (MailDirector.Mail item2 in allMail)
			{
				if (!item2.read)
				{
					hasNewMail = true;
					break;
				}
			}
		}

		public void MailListChanged()
		{
			RefreshNewMail();
			participant.OnMailListChanged();
		}

		public void Push(List<MailDirector.Mail> mail)
		{
			allMail.Clear();
			allMailDict.Clear();
			allMail.AddRange(mail);
		}

		public void Pull(out List<MailDirector.Mail> mail)
		{
			mail = new List<MailDirector.Mail>(allMail);
		}
	}
}
