using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Management.M_Service_Group.Input_Objects;
using ManagemateAPI.Management.M_Service_Group.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Static;
using Microsoft.EntityFrameworkCore;

namespace ManagemateAPI.Management.M_Service_Group.Manager
{
    public class Service_Group_Manager
    {
        private DB_Context _context;
        private readonly IConfiguration _configuration;

        public Service_Group_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Add_Service_Group(Add_Service_Group_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var group_name_check = _context.Service_Group.Where(s => 
                    s.group_name.Equals(input.group_name)
                ).FirstOrDefault();

                if (group_name_check != null)
                {
                    throw new Exception("18");// duplicate
                }

                Service_Group new_record = new Service_Group
                {
                    group_name = input.group_name,
                    tax_pct = input.tax_pct
                };

                _context.Service_Group.Add(new_record);
                _context.SaveChanges();

                return Info.SUCCESSFULLY_ADDED;
            }
        }

        public string Edit_Service_Group(Edit_Service_Group_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var record_to_edit = _context.Service_Group.Where(s => s.id.Equals(input.id)).FirstOrDefault();
                if(record_to_edit == null)
                {
                    throw new Exception("19");// not found
                }

                var record_check = _context.Service_Group.Where(s => 
                    s.group_name.Equals(input.group_name) && 
                    !s.id.Equals(record_to_edit.id)).FirstOrDefault();

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

        public string Delete_Service_Group(Delete_Service_Group_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var record_to_delete = _context.Service_Group.Where(s => 
                    s.id.Equals(input.group_id)
                )
                .Include(g => g.service_list_FK)
                .FirstOrDefault();
                if(record_to_delete == null || record_to_delete.service_list_FK == null)
                {
                    throw new Exception("19");// not found
                }

                if(record_to_delete.service_list_FK.Count > 0)
                {
                    throw new Exception("38");//currently in use
                }

                _context.Service_Group.Remove(record_to_delete);
                _context.SaveChanges();

                return Info.SUCCESSFULLY_DELETED;
            }
        }

        public Service_Group_Model Get_Service_Group_By_Id(Get_Service_Group_By_Id_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var record_to_get = _context.Service_Group.Where(s => s.id.Equals(input.id_to_get)).FirstOrDefault();
                if(record_to_get == null)
                {
                    throw new Exception("19");// not found
                }


                Service_Group_Model return_object = new Service_Group_Model
                {
                    id = record_to_get.id,
                    group_name = record_to_get.group_name,
                    tax_pct = record_to_get.tax_pct
                };

                return return_object;
            }
        }

        public List<Service_Group_Model> Get_All_Service_Group(Session_Data session)
        {
            if (session == null)
            {
                throw new Exception("14");// null error
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var record_list = _context.Service_Group.ToList();

                List<Service_Group_Model> return_obj = new List<Service_Group_Model>();
                
                if(record_list.Count == 0)
                {
                    return return_obj;
                }

                foreach (var group in record_list)
                {
                    return_obj.Add(new Service_Group_Model
                    {
                        id = group.id,
                        tax_pct = group.tax_pct,
                        group_name = group.group_name
                    });
                }


                return return_obj;
            }

        }


    }
}
