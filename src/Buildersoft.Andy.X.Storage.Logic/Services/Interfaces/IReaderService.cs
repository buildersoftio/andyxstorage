using Buildersoft.Andy.X.Storage.Data.Model.Events.Readers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Services.Interfaces
{
    public interface IReaderService
    {
        void StoreReader(ReaderStoredArgs readerStoredArgs);
    }
}
