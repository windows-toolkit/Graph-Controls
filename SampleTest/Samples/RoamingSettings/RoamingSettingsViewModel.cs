﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
using Microsoft.Toolkit.Graph.RoamingSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SampleTest.Samples.RoamingSettings
{
    public class RoamingSettingsViewModel : INotifyPropertyChanged
    {
        private IProvider GlobalProvider => ProviderManager.Instance.GlobalProvider;

        public event PropertyChangedEventHandler PropertyChanged;

        private string _errorText;
        public string ErrorText
        {
            get => _errorText;
            set => Set(ref _errorText, value);
        }

        private UserExtensionDataStore _roamingSettings;

        private ObservableCollection<KeyValuePair<string, object>> _additionalData;
        public ObservableCollection<KeyValuePair<string, object>> AdditionalData
        {
            get => _additionalData;
            set => Set(ref _additionalData, value);
        }

        private string _keyInputText;
        public string KeyInputText
        {
            get => _keyInputText;
            set => Set(ref _keyInputText, value);
        }

        private string _valueInputText;
        public string ValueInputText
        {
            get => _valueInputText;
            set => Set(ref _valueInputText, value);
        }

        private User _me;

        public RoamingSettingsViewModel()
        {
            _roamingSettings = null;
            _keyInputText = string.Empty;
            _valueInputText = string.Empty;
            _me = null;

            ProviderManager.Instance.ProviderUpdated += (s, e) => CheckState();
            CheckState();
        }

        public async void GetValue()
        {
            try
            {
                ErrorText = string.Empty;
                ValueInputText = string.Empty;

                string key = KeyInputText;
                object value = await _roamingSettings.Get(key, false);

                ValueInputText = value.ToString();
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
            }
        }

        public async void SetValue()
        {
            try
            {
                ErrorText = string.Empty;

                await _roamingSettings.Set(KeyInputText, ValueInputText);

                SyncRoamingSettings();
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
            }
        }

        public async void CreateCustomRoamingSettings()
        {
            try
            {
                ErrorText = string.Empty;

                var newExtension = await _roamingSettings.Create();
                AdditionalData = new ObservableCollection<KeyValuePair<string, object>>(newExtension.AdditionalData);

                KeyInputText = string.Empty;
                ValueInputText = string.Empty;
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
            }
        }

        public async void DeleteCustomRoamingSettings()
        {
            try
            {
                ErrorText = string.Empty;

                await _roamingSettings.Delete();

                AdditionalData?.Clear();
                KeyInputText = string.Empty;
                ValueInputText = string.Empty;
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
            }
        }

        public async void SyncRoamingSettings()
        {
            try
            {
                ErrorText = string.Empty;
                AdditionalData?.Clear();

                await _roamingSettings.Sync();
                if (_roamingSettings?.UserExtension?.AdditionalData != null)
                {
                    AdditionalData = new ObservableCollection<KeyValuePair<string, object>>(_roamingSettings.UserExtension.AdditionalData);
                }
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
            }
        }

        private async void CheckState()
        {
            if (GlobalProvider != null && GlobalProvider.State == ProviderState.SignedIn)
            {
                await LoadState();
            }
            else
            {
                ClearState();
            }
        }

        private async Task LoadState()
        {
            try
            {
                _me = await GlobalProvider.Graph.Me.Request().GetAsync();
                string userId = _me.Id;

                _roamingSettings = new CustomRoamingSettings(userId, true);
                KeyInputText = string.Empty;
                ValueInputText = string.Empty;
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
            }
        }

        private void ClearState()
        {
            _me = null;
            _roamingSettings = null;

            KeyInputText = string.Empty;
            ValueInputText = string.Empty;
        }

        private void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}