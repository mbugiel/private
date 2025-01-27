using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Management.M_Lease_Item_Group.Input_Objects;
using ManagemateAPI.Management.M_Lease_Item_Group.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Static;
using Microsoft.EntityFrameworkCore;

/*
 * This is the Item_Type_Manager with methods dedicated to the lease_item_group table.
 * 
 * It contains methods to:
 * add records,
 * edit records,
 * delete records,
 * get record by id,
 * get all the records.
 */
namespace ManagemateAPI.Management.M_Lease_Item_Group.Manager
{
    public class Lease_Item_Group_Manager
    {

        private DB_Context _context;
        private readonly IConfiguration _configuration;


        public Lease_Item_Group_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /* 
         * Add_Lease_Item_Group method
         * This method is used to add new records to the lease_item_group table.
         * 
         * It accepts Add_Lease_Item_Group_Data object as input.
         * It then adds new record with values based on the data given in the input object.
         */
        public string Add_Lease_Item_Group(Add_Lease_Item_Group_Data input_obj, Session_Data session)
        {
            if (input_obj == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();


                var group_name_exist = _context.Lease_Item_Group.Where(i => 
                    i.group_name.Equals(input_obj.group_name) && 
                    i.deleted.Equals(false)
                ).FirstOrDefault();

                if (group_name_exist != null)
                {
                    throw new Exception("18"); // Already exist
                }
                else
                {
                    Lease_Item_Group new_record = new Lease_Item_Group
                    {
                        group_name = input_obj.group_name,
                        rate = input_obj.rate / 100,
                        tax_pct = input_obj.tax_pct,
                        deleted = false
                    };
                    _context.Lease_Item_Group.Add(new_record);
                    _context.SaveChanges();

                    return Info.SUCCESSFULLY_ADDED;
                }

            }

        }

        /* 
         * Edit_Lease_Item_Group method
         * This method is used to edit a record in the lease_item_group table.
         * 
         * It accepts Edit_Lease_Item_Group_Data object as input.
         * It then changes values of a record with those given in the input object only if its ID matches the one in the input object.
         */
        public string Edit_Lease_Item_Group(Edit_Lease_Item_Group_Data input_obj, Session_Data session)
        {
            if (input_obj == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var edited_record = _context.Lease_Item_Group.Where(l => 
                    l.id.Equals(input_obj.group_id) && 
                    l.deleted.Equals(false)
                ).FirstOrDefault();

                if (edited_record != null)
                {
                    var group_exist = _context.Lease_Item_Group.Where(i => 
                        i.group_name.Equals(input_obj.group_name) &&
                        i.rate.Equals(input_obj.rate) &&
                        i.id != edited_record.id
                    ).FirstOrDefault();

                    if (group_exist != null)
                    {
                        throw new Exception("18"); // Already exists
                    }
                    else
                    {
                        edited_record.group_name = input_obj.group_name;
                        edited_record.rate = input_obj.rate / 100;
                        edited_record.tax_pct = input_obj.tax_pct;

                        _context.SaveChanges();

                        return Info.SUCCESSFULLY_CHANGED;
                    }
                }
                else
                {
                    throw new Exception("19"); // edited record not found in db
                }

            }
        }

        /*
         * Delete_Lease_Item_Group method
         * This method is used to a record from the lease_item_group table.
         *  
         * It accepts Delete_Lease_Item_Group_Data object as input.
         * Then it deletes a record if its ID matches the one given in the input object.
         */
        public string Delete_Lease_Item_Group(Delete_Lease_Item_Group_Data input_obj, Session_Data session)
        {
            if (input_obj == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var group_exist = _context.Lease_Item_Group.Where(i => 
                    i.id.Equals(input_obj.group_id) && 
                    i.deleted.Equals(false)
                )
                .Include(g => g.lease_item_in_group_list_FK)
                .FirstOrDefault();

                if (group_exist == null || group_exist.lease_item_in_group_list_FK == null)
                {
                    throw new Exception("19");// item lease group not found
                }

                var group_in_use = group_exist.lease_item_in_group_list_FK.Where(i => i.deleted.Equals(false)).FirstOrDefault();
                if(group_in_use != null)
                {
                    throw new Exception("37");// group in use
                }

                group_exist.deleted = true;

                _context.SaveChanges();

                return Info.SUCCESSFULLY_DELETED;
            }
        }

        /*
         * Get_Lease_Item_Group_By_Id method
         * This method gets a record from the lease_item_group table by its ID and returns it.
         * 
         * It accepts Get_Lease_Item_Group_By_Id_Data object as input.
         * Then it gets a records that has the same ID as the ID given in the input object
         */
        public Lease_Item_Group_Model Get_Lease_Item_Group_By_Id(Get_Lease_Item_Group_By_Id_Data input_obj, Session_Data session)
        {

            if (input_obj == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var lease_item_group = _context.Lease_Item_Group.Where(c => 
                    c.id.Equals(input_obj.id_to_get) &&
                    c.deleted.Equals(false)
                ).FirstOrDefault();

                if (lease_item_group == null)
                {
                    throw new Exception("19");// item lease group not found
                }

                Lease_Item_Group_Model lease_item_group_model = new Lease_Item_Group_Model
                {
                    id = lease_item_group.id,
                    group_name = lease_item_group.group_name,
                    rate = lease_item_group.rate * 100,
                    tax_pct = lease_item_group.tax_pct
                };

                return lease_item_group_model;

            }

        }

        /*
         * Get_All_Lease_Item_Group method
         * This method gets all of the records in the lease_item_group table and returns them in a list.
         * 
         * It accepts Get_All_Lease_Item_Group_Data object as input.
         */
        public List<Lease_Item_Group_Model> Get_All_Lease_Item_Group(Session_Data session)
        {
            if (session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var lease_item_group_list = _context.Lease_Item_Group.Where(i => i.deleted.Equals(false)).ToList();

                List<Lease_Item_Group_Model> lease_item_group_list_model = new List<Lease_Item_Group_Model>();

                if (lease_item_group_list.Count == 0)
                {
                    return lease_item_group_list_model;
                }

                foreach (var group in lease_item_group_list)
                {
                    lease_item_group_list_model.Add(new Lease_Item_Group_Model { id = group.id, rate = group.rate * 100, tax_pct = group.tax_pct, group_name = group.group_name });
                }



                return lease_item_group_list_model;
            }

        }



    }
}