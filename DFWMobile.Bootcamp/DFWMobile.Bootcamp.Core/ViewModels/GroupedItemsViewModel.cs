﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;
using DFWMobile.Bootcamp.Common.DataSources;
using DFWMobile.Bootcamp.Common.Models;
using DFWMobile.Bootcamp.Common.Services;
using DFWMobile.Bootcamp.Common.Settings;
using DFWMobile.Bootcamp.Core.Helpers;

namespace DFWMobile.Bootcamp.Core.ViewModels
{
    public class GroupedItemsViewModel
        : BaseViewModel
    {
        private IDataServiceFactory _dataServiceFactory;
        private readonly List<IDataService> _dataServices;
        private readonly IMvxResourceLoader _resourceLoader;
        private readonly ObservableCollection<Group<Item>> _groupedItems; 
        public GroupedItemsViewModel(IAppSettings appSettings, IDataServiceFactory dataServiceFactory, IMvxResourceLoader resourceLoader)
            : base(appSettings)
        {
            _dataServiceFactory = dataServiceFactory;
            _resourceLoader = resourceLoader;

            _dataServices = new List<IDataService>();
            foreach (var source in DataServiceFactoryHelper.DataSources)
            {
                _dataServices.Add(_dataServiceFactory.GenerateService(source));
            }

            _groupedItems = new ObservableCollection<Group<Item>>();
        }

        public async void Init()
        {
            IsBusy = true;
            foreach (var service in _dataServices)
            {
                var items = await service.GetItems();

                if (items != null)
                {
                    var group = new Group<Item>(service.Source.ServiceName, items);

                    _groupedItems.Add(group);
                }
            }
            IsBusy = false;
        }

        public ObservableCollection<Group<Item>> ItemGroups
        {
            get { return _groupedItems; }
        }

        private ICommand _goToGroupCommand;

        public ICommand GoToGroupCommand
        {
            get
            {
                return (_goToGroupCommand = _goToGroupCommand ??
                                            new MvxCommand<string>(GoToGroupDetails));
            }
        }

        private ICommand _goToItemCommand;

        public ICommand GoToItemCommand
        {
            get
            {
                return (_goToItemCommand = _goToItemCommand ??
                                            new MvxCommand<Item>(
                                                (item) => GoToGroupDetails(item)));
            }
        }

        private void GoToGroupDetails(Item item)
        {
            ShowViewModel<GroupDetailsViewModel>(new { group = item.Group, title = item.Title });
        }

        private void GoToGroupDetails(string groupName)
        {
            ShowViewModel<GroupDetailsViewModel>(new { group = groupName });
        }
    }
}
