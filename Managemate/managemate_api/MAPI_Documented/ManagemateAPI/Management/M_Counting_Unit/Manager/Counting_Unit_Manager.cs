using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Management.M_Counting_Unit.Input_Objects;
using ManagemateAPI.Management.M_Counting_Unit.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Static;

/*
 * This is the Item_Counting_Type_Manager with methods dedicated to the Item_Counting_Type table.
 * 
 * It contains methods to:
 * add records,
 * edit records,
 * delete records,
 * get record by id,
 * get all the records.
 */
namespace ManagemateAPI.Management.M_Counting_Unit.Manager
{
    public class Counting_Unit_Manager
    {

        private DB_Context _context;
        private readonly IConfiguration _configuration;


        public Counting_Unit_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /* 
         * Add_Counting_Unit method
         * This method is used to add new records to the Counting_Unit table.
         * 
         * It accepts Add_Counting_Unit_Data object as input.
         * It then adds new record with values based on the data given in the input object.
         */
        public string Add_Counting_Unit(Add_Counting_Unit_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var counting_unit_exist = _context.Counting_Unit.Where(i => i.unit.Equals(input_obj.unit)).FirstOrDefault();

                if (counting_unit_exist != null)
                {
                    throw new Exception("18"); // Already exist
                }
                else
                {
                    Counting_Unit new_record = new Counting_Unit
                    {
                        unit = input_obj.unit
                    };
                    _context.Counting_Unit.Add(new_record);
                    _context.SaveChanges();

                    return Info.SUCCESSFULLY_ADDED;
                }

            }

        }

        /* 
         * Edit_Counting_Unit method
         * This method is used to edit a record in the Counting_Unit table.
         * 
         * It accepts Edit_Counting_Unit_Data object as input.
         * It then changes values of a record with those given in the input object only if its ID matches the one in the input object.
         */
        public string Edit_Counting_Unit(Edit_Counting_Unit_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var edited_record = _context.Counting_Unit.Where(c => c.id.Equals(input_obj.unit_id)).FirstOrDefault();

                if (edited_record != null)
                {
                    var counting_unit_exist = _context.Counting_Unit.Where(i => i.unit.Equals(input_obj.unit) && i.id != edited_record.id).FirstOrDefault();

                    if (counting_unit_exist != null)
                    {
                        throw new Exception("18"); // Already in use
                    }
                    else
                    {
                        edited_record.unit = input_obj.unit;

                        _context.SaveChanges();

                        return Info.SUCCESSFULLY_CHANGED;
                    }
                }
                else
                {
                    throw new Exception("19");
                }

            }
        }

        /*
         * Delete_Counting_Unit method
         * This method is used to a record from the Counting_Unit table.
         *  
         * It accepts Delete_Counting_Unit_Data object as input.
         * Then it deletes a record if its ID matches the one given in the input object.
         */
        public string Delete_Counting_Unit(Delete_Counting_Unit_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var counting_unit_exist = _context.Counting_Unit.Where(i => i.id.Equals(input_obj.unit_id) && i.id > 0).FirstOrDefault();

                if (counting_unit_exist == null)
                {
                    throw new Exception("19");// counting unit not found
                }
                else
                {
                    _context.Counting_Unit.Remove(counting_unit_exist);
                    _context.SaveChanges();

                    return Info.SUCCESSFULLY_DELETED;
                }

            }
        }

        /*
         * Get_Counting_Unit_By_Id method
         * This method gets a record from the Counting_Unit table by its ID and returns it.
         * 
         * It accepts Get_Counting_Unit_By_Id_Data object as input.
         * Then it gets a records that has the same ID as the ID given in the input object
         */
        public Counting_Unit_Model Get_Counting_Unit_By_Id(Get_Counting_Unit_By_Id_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var counting_unit = _context.Counting_Unit.Where(c => c.id.Equals(input_obj.unit_id)).FirstOrDefault();

                if (counting_unit == null)
                {
                    throw new Exception("19");// counting unit not found
                }
                else
                {
                    Counting_Unit_Model item_counting_type_model = new Counting_Unit_Model { id = counting_unit.id, unit = counting_unit.unit };

                    return item_counting_type_model;
                }

            }

        }

        /*
         * Get_All_Item_Counting_Type method
         * This method gets all of the records in the Item_Counting_Type table and returns them in a list.
         * 
         * It accepts Get_All_Item_Counting_Type_Data object as input.
         */
        public List<Counting_Unit_Model> Get_All_Item_Counting_Type(Session_Data session)
        {
            if (session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var counting_unit_list = _context.Counting_Unit.ToList();

                if (counting_unit_list == null)
                {
                    throw new Exception("19");// counting units not found
                }
                else
                {
                    List<Counting_Unit_Model> counting_unit_list_model = new List<Counting_Unit_Model>();

                    foreach (var unit in counting_unit_list)
                    {
                        counting_unit_list_model.Add(new Counting_Unit_Model { id = unit.id, unit = unit.unit });
                    }

                    return counting_unit_list_model;

                }


            }

        }



    }
}
