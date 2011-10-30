using MindTouch.Xml;

namespace Usivity.Data.Entities {

    public interface IEntity {

        //--- Properties ---
        string Id { get; }
   
        //--- Methods ---
        XDoc ToDocument(string relation = null);
    }
}
