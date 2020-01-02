// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AcoustID.Web;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Leacme.Lib.MusicMulcher;

namespace Leacme.App.MusicMulcher {

	public class AppUI {

		private StackPanel rootPan = (StackPanel)Application.Current.MainWindow.Content;
		private Library lib = new Library();

		public AppUI() {

			var blurb1 = App.TextBlock;
			blurb1.Text = "Open an audio file to attempt to identify it.";
			blurb1.TextAlignment = TextAlignment.Center;

			rootPan.Spacing = 10;

			var resultGd = App.DataGrid;
			resultGd.Items = new List<Recording>() { };

			var openF = App.HorizontalFieldWithButton;
			openF.holder.HorizontalAlignment = HorizontalAlignment.Center;
			openF.label.Text = "Audio file:";
			openF.field.IsReadOnly = true;
			openF.field.Width = 600;
			openF.button.Content = "Open File...";
			openF.button.Click += async (z, zz) => {
				var afPath = await OpenFile(); if (!string.IsNullOrWhiteSpace(afPath)) {
					var tempB = blurb1.Text;
					openF.field.Text = afPath;
					var aResp = new List<LookupResponse>();
					try {
						((App)Application.Current).LoadingBar.IsIndeterminate = true;
						blurb1.Text = "Identifying audio...";
						aResp = await lib.ProcessAudioFile(new Uri(afPath));
						((App)Application.Current).LoadingBar.IsIndeterminate = false;
						blurb1.Text = tempB;
					} catch (Exception e) {
						((App)Application.Current).LoadingBar.IsIndeterminate = false;
						blurb1.Text = tempB;
						Console.WriteLine(e);
					}

					if (aResp.Any()) {
						resultGd.Items = aResp.SelectMany(zzz => { return zzz.Results.SelectMany(zzzz => { return zzzz.Recordings; }); }).ToList();
					} else {
						blurb1.Text = "No matches found.";
						DispatcherTimer.RunOnce(() => {
							blurb1.Text = tempB;
						}, new TimeSpan(0, 0, 0, 5, 0));
					}

				}
			};
			rootPan.Children.AddRange(new List<IControl> { blurb1, openF.holder, resultGd });

		}

		private async Task<string> OpenFile() {
			var dialog = new OpenFileDialog() {
				Title = "Open Audio File",
				InitialDirectory = Directory.GetCurrentDirectory(),
				AllowMultiple = false,
			};
			var res = await dialog.ShowAsync(Application.Current.MainWindow);
			return (res?.Any() == true) ? res[0] : "";
		}
	}
}