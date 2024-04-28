﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using StatifyUWPLib;
using SpotifyAPI.Web;
using Windows.ApplicationModel.DataTransfer;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Statify
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Auth.isAuthorized)
            {
                this.Frame.Navigate(typeof(InterfacePage), Auth.AccessToken);
            }
            else
            {
                (string verifier, string challenge) = Auth.VerifierAndChallenge;
                DataPackage pkg = new DataPackage();
                pkg.SetText(verifier);
                Clipboard.SetContent(pkg);
                var loginRequest = new LoginRequest(
                  new Uri("http://localhost:5543/callback"),
                  Auth.clientID,
                  LoginRequest.ResponseType.Code
                )
                {
                    CodeChallengeMethod = "S256",
                    CodeChallenge = challenge,
                    Scope = new[] { Scopes.UserTopRead }
                };
                var uri = loginRequest.ToUri();
                this.Frame.Navigate(typeof(AuthWebPage), uri);
            }
        }
    }
}