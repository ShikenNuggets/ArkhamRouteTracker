using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArkhamDisplay{
	public partial class App : Application{
        protected override void OnStartup(StartupEventArgs e){
            base.OnStartup(e);
            ChangeSkin(Theme.Dark);
        }

        public void ChangeSkin(Theme newTheme){
            Resources.Clear();
            Resources.MergedDictionaries.Clear();
            if(newTheme == Theme.Light){
                ApplyResources("LightTheme.xaml");
            }else if (newTheme == Theme.Dark){
                ApplyResources("DarkTheme.xaml");
            }
        }

        private void ApplyResources(string src){
            var dict = new ResourceDictionary(){ Source = new Uri(src, UriKind.Relative) };
            foreach(var mergeDict in dict.MergedDictionaries){
                Resources.MergedDictionaries.Add(mergeDict);
            }

            foreach(var key in dict.Keys){
                Resources[key] = dict[key];
            }
        }
    }
}