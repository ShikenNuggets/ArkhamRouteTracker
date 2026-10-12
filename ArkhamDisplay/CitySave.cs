using System;

namespace ArkhamDisplay {
	public class CitySave : SaveParser {
		protected ArkhamSaveParser.ArkhamCitySave? m_citySave;

		public CitySave(string filePath, int id) : base(filePath, id) {
			
		}

		protected override string Decompress() {
			string result = base.Decompress();

			try {
				var compressedSave = new ArkhamSaveParser.CompressedSave(m_rawCompressedBytes);
				m_citySave = new ArkhamSaveParser.ArkhamCitySave(compressedSave);
			} catch { } // TODO: Error handling

			return result;
		}

		private static bool TryParsePhysicalChallengeProgressPair(string input, out uint a, out uint b) {
			a = 0;
			b = 0;

			if (string.IsNullOrWhiteSpace(input)) {
				return false;
			}

			string[] parts = input.Split('_');
			return parts.Length == 2 && uint.TryParse(parts[0], out a) && uint.TryParse(parts[1], out b);
		}

		public override bool HasKey(Entry entry, int requiredMatches) {
			uint physicalChallengeId = 0;
			if (m_citySave != null && "[PARSER] Physical Challenge Done".Equals(entry.type) && uint.TryParse(entry.id, out physicalChallengeId)) {
				return m_citySave.GetPhysicalChallengeInfo((ArkhamSaveParser.PhysicalChallenges)physicalChallengeId).beaten;
			}

			if (m_citySave != null && "[PARSER] Physical Challenge Progress".Equals(entry.type) && TryParsePhysicalChallengeProgressPair(entry.id, out physicalChallengeId, out uint physicalChallengeProgress)) {
				var challengeInfo = m_citySave.GetPhysicalChallengeInfo((ArkhamSaveParser.PhysicalChallenges)physicalChallengeId);
				return challengeInfo.bestValue >= physicalChallengeProgress || challengeInfo.beaten;
			}

			return base.HasKey(entry, requiredMatches);
		}
	}
}
