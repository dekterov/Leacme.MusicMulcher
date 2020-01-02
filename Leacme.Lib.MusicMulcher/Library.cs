// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AcoustID;
using AcoustID.Chromaprint;
using AcoustID.Web;

namespace Leacme.Lib.MusicMulcher {

	public class Library {

		public Library() {
			AcoustID.Configuration.ClientKey = "tpWYI7pszq";
		}

		/// <summary>
		/// Fingerpring the audio and send it to the AcoustID identification service for results.
		/// /// </summary>
		/// <param name="pathToAudioFile"></param>
		/// <returns>The AcoustID identification response object with the audio information if found.</returns>
		public async Task<List<LookupResponse>> ProcessAudioFile(Uri pathToAudioFile) {
			short[] auData = LoadAudioFile(pathToAudioFile);
			int tSec = (int)TagLib.File.Create(pathToAudioFile.LocalPath).Properties.Duration.TotalSeconds;
			ChromaContext ctx = new ChromaContext(ChromaprintAlgorithm.TEST2);
			var responces = new List<LookupResponse>();

			var commonSamples = new List<int> { 8000, 11025, 16000, 22050, 44100, 48000, 88200, 96000, 176400, 192000, 352800, 384000 };

			async Task FingerprintAndLookup(int sample, int channels) {
				try {
					ctx.Start(sample, channels);
					ctx.Feed(auData, auData.Length);
					ctx.Finish();
				} catch {
					Console.WriteLine("Failed on fingerprint audio sample for " + sample + "hz sampling rate " + channels + "channels.");
				}

				try {
					var lur = await new AcoustID.Web.LookupService().GetAsync(ctx.GetFingerprint(), tSec);
					if (lur.Results.Any()) {
						responces.Add(lur);
					}
				} catch {
					Console.WriteLine("Failed on query audio sample for " + sample + "hz sampling rate " + channels + "channels.");
				}
			}
			foreach (var sample in commonSamples) {
				await FingerprintAndLookup(sample, 1);
				await FingerprintAndLookup(sample, 2);
			}
			return responces;
		}

		/// <summary>
		/// Read the audio file into an array of shorts for the use by the fingerprinting service.
		/// /// </summary>
		/// <param name="pathToAudioFile"></param>
		/// <returns></returns>
		public short[] LoadAudioFile(Uri pathToAudioFile) {
			byte[] bytes = File.ReadAllBytes(pathToAudioFile.LocalPath);
			return Array.ConvertAll(bytes, z => (short)z);
		}
	}
}