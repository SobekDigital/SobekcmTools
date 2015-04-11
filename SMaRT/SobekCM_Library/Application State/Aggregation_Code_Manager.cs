#region Using directives

using System.Collections.Generic;
using System.Collections.ObjectModel;
using SobekCM.Library.Aggregations;

#endregion

namespace SobekCM.Library.Application_State
{
    /// <summary>
    ///   Code manager maintains a list of all the valid aggregation codes and
    ///   also provides a lookup from aggregation code to greenstone full text collection code
    ///   to allow for full text searching of single items from the item viewer.
    /// </summary>
    public class Aggregation_Code_Manager
    {
        private readonly Dictionary<string, Item_Aggregation_Related_Aggregations> aggregationsByCode;
        private readonly Dictionary<int, List<Item_Aggregation_Related_Aggregations>> aggregationsByThematicheading;
        private readonly Dictionary<string, List<Item_Aggregation_Related_Aggregations>> aggregationsByType;
        private readonly List<Item_Aggregation_Related_Aggregations> allAggregations;
        private readonly List<string> allTypes;

        /// <summary>
        ///   Constructor for a new instance of the Aggregation_Code_Manager class
        /// </summary>
        public Aggregation_Code_Manager()
        {
            // Declare the collections
            aggregationsByThematicheading = new Dictionary<int, List<Item_Aggregation_Related_Aggregations>>();
            aggregationsByType = new Dictionary<string, List<Item_Aggregation_Related_Aggregations>>();
            aggregationsByCode = new Dictionary<string, Item_Aggregation_Related_Aggregations>();
            allTypes = new List<string>();
            allAggregations = new List<Item_Aggregation_Related_Aggregations>();
        }

        /// <summary>
        ///   Read-only collection of all the aggregation information
        /// </summary>
        public ReadOnlyCollection<Item_Aggregation_Related_Aggregations> All_Aggregations
        {
            get { return new ReadOnlyCollection<Item_Aggregation_Related_Aggregations>(allAggregations); }
        }

        /// <summary>
        ///   Read-only collection of all the aggregation types
        /// </summary>
        public ReadOnlyCollection<string> All_Types
        {
            get { return new ReadOnlyCollection<string>(allTypes); }
        }

        /// <summary>
        ///   Gets the number of different aggregation types present
        /// </summary>
        public int Types_Count
        {
            get { return allTypes.Count; }
        }

        /// <summary>
        ///   Gets the aggregation information by aggregation code
        /// </summary>
        /// <param name = "Aggregation_Code"> Code for the aggregation of interest</param>
        /// <returns> Aggregation information, or NULL if not present </returns>
        public Item_Aggregation_Related_Aggregations this[string Aggregation_Code]
        {
            get {
                return aggregationsByCode.ContainsKey(Aggregation_Code.ToUpper()) ? aggregationsByCode[Aggregation_Code.ToUpper()] : null;
            }
        }

        /// <summary>
        ///   Clears the internal data for this code manager
        /// </summary>
        internal void Clear()
        {
            aggregationsByThematicheading.Clear();
            aggregationsByType.Clear();
            aggregationsByCode.Clear();
            allTypes.Clear();
            allAggregations.Clear();
        }

        internal void Add_Collection(Item_Aggregation_Related_Aggregations New_Aggregation, int Theme)
        {
            // Add this to the various collections
            aggregationsByCode[New_Aggregation.Code] = New_Aggregation;
            allAggregations.Add(New_Aggregation);
            if (!allTypes.Contains(New_Aggregation.Type))
            {
                allTypes.Add(New_Aggregation.Type);
            }
            if (aggregationsByType.ContainsKey(New_Aggregation.Type))
            {
                aggregationsByType[New_Aggregation.Type].Add(New_Aggregation);
            }
            else
            {
                aggregationsByType[New_Aggregation.Type] = new List<Item_Aggregation_Related_Aggregations> {New_Aggregation};
            }
            if (Theme > 0)
            {
                if (aggregationsByThematicheading.ContainsKey(Theme))
                {
                    aggregationsByThematicheading[Theme].Add(New_Aggregation);
                }
                else
                {
                    aggregationsByThematicheading[Theme] = new List<Item_Aggregation_Related_Aggregations> {New_Aggregation};
                }
            }
        }

        /// <summary>
        ///   Read-only collection of item aggregations matching a particular aggregation type
        /// </summary>
        /// <param name = "AggregationType"> Type of aggregations to return </param>
        /// <returns> Read-only collection of item aggregation relational objects </returns>
        public ReadOnlyCollection<Item_Aggregation_Related_Aggregations> Aggregations_By_Type(string AggregationType)
        {
            if (aggregationsByType.ContainsKey(AggregationType))
            {
                return new ReadOnlyCollection<Item_Aggregation_Related_Aggregations>(aggregationsByType[AggregationType]);
            }
            
            return new ReadOnlyCollection<Item_Aggregation_Related_Aggregations>( new List<Item_Aggregation_Related_Aggregations>());
        }

        /// <summary>
        ///   Read-only collection of item aggregations matching a particular thematic heading id
        /// </summary>
        /// <param name = "ThemeID"> Primary key for the thematic heading to pull </param>
        /// <returns> Read-only collection of item aggregation relational objects </returns>
        public ReadOnlyCollection<Item_Aggregation_Related_Aggregations> Aggregations_By_ThemeID(int ThemeID)
        {
            if (aggregationsByThematicheading.ContainsKey(ThemeID))
            {
                return new ReadOnlyCollection<Item_Aggregation_Related_Aggregations>(aggregationsByThematicheading[ThemeID]);
            }
            
            return new ReadOnlyCollection<Item_Aggregation_Related_Aggregations>(new List<Item_Aggregation_Related_Aggregations>());
        }

        /// <summary>
        ///   Gets the short name associated with a provided aggregation code
        /// </summary>
        /// <param name = "Aggregation_Code"> Code for the aggregation of interest</param>
        /// <returns> Short name of valid aggregation, otehrwise the aggregation code is returned </returns>
        public string Get_Collection_Short_Name(string Aggregation_Code)
        {
            if (aggregationsByCode.ContainsKey(Aggregation_Code.ToUpper()))
                return aggregationsByCode[Aggregation_Code.ToUpper()].ShortName;
            
            return Aggregation_Code;
        }

        /// <summary>
        ///   Checks to see if an aggregation code exists
        /// </summary>
        /// <param name = "Aggregation_Code"> Code for the aggregation of interest </param>
        /// <returns> TRUE if the aggregation exists, otherwise FALSE </returns>
        public bool isValidCode(string Aggregation_Code)
        {
            return aggregationsByCode.ContainsKey(Aggregation_Code.ToUpper());
        }
    }
}