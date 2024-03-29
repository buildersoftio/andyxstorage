﻿using Buildersoft.Andy.X.Storage.Model.App.Topics;
using System;
using System.Collections.Concurrent;

namespace Buildersoft.Andy.X.Storage.Model.App.Components
{
    public class Component
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public ConcurrentDictionary<string, Topic> Topics { get; set; }

        public ComponentSettings Settings { get; set; }


        public Component()
        {
            Topics = new ConcurrentDictionary<string, Topic>();
            Settings = new ComponentSettings();
        }
    }
}
