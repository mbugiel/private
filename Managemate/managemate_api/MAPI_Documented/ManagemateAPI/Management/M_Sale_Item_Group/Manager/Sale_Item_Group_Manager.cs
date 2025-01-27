using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Management.M_Sale_Item_Group.Input_Objects;
using ManagemateAPI.Management.M_Sale_Item_Group.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Static;
using Microsoft.EntityFrameworkCore;

namespace ManagemateAPI.Management.M_Sale_Item_Group.Manager
{
    public class Sale_Item_Group_Manager
    {
        private DB_Context _context;
        private readonly IConfiguration _configuration;

        public Sale_Item_Group_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Add_Sale_Item_Group(Add_Sale_Item_Group_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var group_name_check = _context.Sale_Item_Group.Where(s => 
                    s.group_name.Equals(input.group_name) &&
                    s.deleted.Equals(false)
                ).FirstOrDefault();

                if (group_name_check != null)
                {
                    throw new Exception("18");//duplicate
                }

                Sale_Item_Group new_record = new Sale_Item_Group
                {
                    group_name = input.group_name,
                    tax_pct = input.tax_pct,
                    deleted = false
                };

                _context.Sale_Item_Group.Add(new_record);
                _context.SaveChanges();

                return Info.SUCCESSFULLY_ADDED;
            }
        }

        public string Edit_Sale_Item_Group(Edit_Sale_Item_Group_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var record_to_edit = _context.Sale_Item_Group.Where(s => 
                    s.id.Equals(input.id) &&
                    s.deleted.Equals(false)
                ).FirstOrDefault();

                if(record_to_edit == null)
                {
                    throw new Exception("19");// not found
                }


                var record_check = _context.Sale_Item_Group.Where(s => 
                    s.group_name.Equals(input.group_name) &&
                    s.tax_pct.Equals(input.tax_pct) &&
                    !s.id.Equals(record_to_edit.id) &&
                    s.deleted.Equals(false)
                ).FirstOrDefault();

                if (record_check != null)
                {
                    throw new Exception("18");//duplicate
                }

                record_to_edit.group_name = input.group_name;
                record_to_edit.tax_pct = input.tax_pct;

                _context.SaveChanges();

                return Info.SUCCESSFULLY_CHANGED;
            }
        }

        public string Delete_Sale_Item_Group(Delete_Sale_Item_Group_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var record_to_delete = _context.Sale_Item_Group.Where(s => 
                    s.id.Equals(input.group_id) &&
                    s.deleted.Equals(false)
                )
                .Include(g => g.sale_item_in_group_list_FK)
                .FirstOrDefault();

                if (record_to_delete == null || record_to_delete.sale_item_in_group_list_FK == null)
                {
                    throw new Exception("19");// not found
                }

                var group_in_use = record_to_delete.sale_item_in_group_list_FK.Where(i => i.deleted.Equals(false)).FirstOrDefault();
                if (group_in_use != null)
                {
                    throw new Exception("37");// group in use
                }

                record_to_delete.deleted = true;

                _context.SaveChanges();

                return Info.SUCCESSFULLY_DELETED;
            }
        }

        public Sale_Item_Group_Model Get_Sale_Item_Group_By_Id(Get_Sale_Item_Group_By_Id_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var record_to_get = _context.Sale_Item_Group.Where(s => 
                    s.id.Equals(input.id_to_get) &&
                    s.deleted.Equals(false)
                ).FirstOrDefault();

                if(record_to_get == null)
                {
                    throw new Exception("19");//not found
                }

                Sale_Item_Group_Model return_object = new Sale_Item_Group_Model
                {
                    id = record_to_get.id,
                    group_name = record_to_get.group_name,
                    tax_pct = record_to_get.tax_pct
                };

                return return_object;
            }
        }

        public List<Sale_Item_Group_Model> Get_All_Sale_Item_Group(Session_Data session)
        {
            if(session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                //WHERE DELETED = FALSE
                var record_list = _context.Sale_Item_Group.Where(sg => sg.deleted.Equals(false)).ToList();

                List<Sale_Item_Group_Model> return_obj = new List<Sale_Item_Group_Model>();

                if (record_list.Count == 0)
                {
                    return return_obj;
                }

                foreach (var group in record_list)
                {
                    if (group == null)
                    {
                        throw new Exception("19"); //not found
                    }

                    return_obj.Add(
                        new Sale_Item_Group_Model
                        {
                            id = group.id,
                            tax_pct = group.tax_pct,
                            group_name = group.group_name
                        }
                    );
                }
                                

                return return_obj;
            }
        }


    }
}
