using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceanApp.Core;
using OceanApp.Dto;
using OceanApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace OceanApp.Controllers
{
    public class HomeController : Controller
    {
        [HttpPost("MakeTransfer")]
        [AllowAnonymous]
        public async Task<IActionResult> MakeTransfer([FromBody] TransferBody transferBody)
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    string BLNO = "";
                    string BLID = "";
                    string BLDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    string From_Br = transferBody.FromBranch.ID.ToString();
                    string To_Br = transferBody.ToBranch.ID.ToString();
                    string BLTime = DateTime.Now.ToString("hh:mm:ss.ms");
                    string TotalQty = transferBody.Items.Count().ToString();

                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;

                    //cmd.CommandText = "SELECT TOP 1 * FROM BSR WHERE BSTID = 4 AND Branch = " + From_Br;
                    cmd.CommandText = "SELECT TOP 1 * FROM BSR WHERE BSTID = 11 AND Branch = " + From_Br;
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        BLID = reader["BLID"].ToString();
                    }
                    else
                        return Ok(false);
                    reader.Close();

                    if (BLID != null)
                    { 
                        if (BLID.Trim().Length > 0)
                        {

                            cmd.CommandText = "SELECT MAX(BLNO)+1 AS MaxBLNO FROM DTI WHERE BLID = "+"'"+BLID+"'";
                            reader = cmd.ExecuteReader();
                            if (reader.HasRows)
                            {
                                reader.Read();
                                BLNO = reader["MaxBLNO"].ToString();
                            }
                            reader.Close();

                            if (BLNO != null)
                            {

                                if (BLNO.Trim().Length > 0)
                                {
                                    if (transferBody.Items != null)
                                    {
                                        if (transferBody.Items.Count > 0)
                                        {                                            

                                            DateTime d = DateTime.Now;
                                            string currentblDate = d.Year.ToString() + "-" + d.Month.ToString() + "-" + d.Day.ToString();
                                            string currentblTime = "1900-01-01 " + d.Hour.ToString() + ":" + d.Minute.ToString() + ":" + d.Second.ToString() + ".000";
                                            string currentRDate = currentblDate + " " + d.Hour.ToString() + ":" + d.Minute.ToString() + ":" + d.Second.ToString() + ".000";
                                            string currentBlno = BLNO;
                                            string currentBlid = BLID;
                                            string currentTotalValue = transferBody.Items.Sum(i => (double.Parse(i.Price) * int.Parse(i.Qty))).ToString();
                                            string currentQty = transferBody.Items.Count.ToString();
                                            string currentBranchId = transferBody.FromBranch.ID.ToString();
                                            string currentCurrencyId = transferBody.Currency.ID.ToString();
                                            string currentRate = "1";
                                            string currentUsername = transferBody.Username;
                                            string currentToBranch = transferBody.ToBranch.ID.ToString(); ;
                                            string currentUserCode = transferBody.UserId;
                                            string currentSysUser = transferBody.UserId;


                                            cmd.CommandType = System.Data.CommandType.Text;
                                            cmd.CommandTimeout = 60000;

                                            cmd.CommandText = "INSERT INTO DTI(BLID, BLNO, From_Br, To_Br, BLDate, TotalQty, User_Code, BPost, QPost, UserSys, BLTime, FullUserName, IsOrder) " +
                                                "VALUES(" + currentBlid + "," + currentBlno + "," + currentBranchId + "," + currentToBranch + ",'" + currentblDate + "'," + currentQty.ToString() + "," + currentUserCode + ",0,0," + currentSysUser + ",'" + currentblTime + "','" + currentUsername + "'," + "0)";
                                            string c = cmd.CommandText;
                                            cmd.ExecuteNonQuery();


                                            DataTable dtdDataTable;
                                            using (dtdDataTable = new DataTable())
                                            {
                                                dtdDataTable.Columns.Add("BLNO", typeof(int));
                                                dtdDataTable.Columns.Add("BLID", typeof(int));
                                                dtdDataTable.Columns.Add("Number", typeof(int));
                                                dtdDataTable.Columns.Add("Code", typeof(int));
                                                dtdDataTable.Columns.Add("Qty", typeof(int));
                                                dtdDataTable.Columns.Add("Price", typeof(float));
                                                dtdDataTable.Columns.Add("CostPrice", typeof(float));
                                                dtdDataTable.Columns.Add("ComputerNo", typeof(string));
                                                dtdDataTable.Columns.Add("From_Br", typeof(string));
                                                dtdDataTable.Columns.Add("To_Br", typeof(int));
                                                dtdDataTable.Columns.Add("PriceType", typeof(int));

                                                int i = 0;
                                                foreach (var item in transferBody.Items)
                                                {
                                                    i = i + 1;
                                                    string computerNo = item.ComputerNo;
                                                    string code = item.Code;
                                                    string price = item.Price;

                                                    dtdDataTable.Rows.Add(currentBlno, currentBlid, i + 1, code, 1, Convert.ToDouble(price), 0.0, computerNo, currentBranchId, currentToBranch, 0);
                                                }

                                            }
                                            var pList = new SqlParameter("@dtd", SqlDbType.Structured);
                                            pList.Value = dtdDataTable;
                                            pList.TypeName = "dbo.dtdType";
                                            cmd.Parameters.Add(pList);
                                            cmd.Connection = conn;
                                            cmd.CommandTimeout = 60000;
                                            cmd.CommandText = @"insert into DTD(BLNO, BLID, Number, Code, Qty, Price, CostPrice, ComputerNo, From_Br, To_Br, PriceType) " +
                                                " SELECT BLNO, BLID, Number, Code, Qty, Price, CostPrice, ComputerNo, From_Br, To_Br, PriceType FROM @dtd as x";
                                            cmd.ExecuteNonQuery();

                                        }
                                        else
                                            return Ok(false);
                                    }
                                    else
                                        return Ok(false);
                                }
                                else
                                    return Ok(false);
                            }
                            else
                                return Ok(false);
                        }
                        else
                            return Ok(false);
                    }
                    else
                        return Ok(false);

                }
                return Ok(true);
            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }

        [HttpGet("GetBranches")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    List<Branch> branches = new List<Branch>();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = "SELECT * FROM Branches";
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Branch branch = new Branch();
                            branch.ID = reader["BranchId"].ToString();
                            branch.ArabicName = reader["BranchName"].ToString();
                            branch.EnglishName = reader["BranchEName"].ToString();
                            branches.Add(branch);
                        }                        
                    }

                    reader.Close();
                    return Ok(branches);
                }
            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }

        [HttpPost("FindItem")]
        [AllowAnonymous]
        public async Task<IActionResult> FindItem([FromBody] SearchBody searchBody)
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    string code = "";
                    string computerNo = "";
                    string price = "";
                    string sale = "";
                    string localcost = "";
                    string barcode = "";
                    string color = "";
                    string size = "";
                    string branch = "";
                    string branchId = "";
                    int qty = 0;

                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    if (searchBody.searchText != "")
                    {

                        if ((searchBody.branchId != null) && (searchBody.branchId != "") && (searchBody.branchId != "0"))
                            cmd.CommandText = "SELECT * FROM QMTS WHERE (Barcode = '"+searchBody.searchText+"' OR ComputerNo = '" + searchBody.searchText + "') AND BranchId = '" + searchBody.branchId.ToString()+"'";
                        else
                            cmd.CommandText = "SELECT * FROM QMTS WHERE (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "')";

                        if ((searchBody.colorId != null) && (searchBody.colorId != "") && (searchBody.colorId != "0"))
                        {
                            if ((searchBody.branchId != null) && (searchBody.branchId != "") && (searchBody.branchId != "0"))
                                cmd.CommandText = "SELECT * FROM QMTS WHERE (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "') AND ColorId = '" + searchBody.colorId.ToString() + "' AND BranchId = '"+searchBody.branchId.ToString()+"'";
                            else
                                cmd.CommandText = "SELECT * FROM QMTS WHERE (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "') AND ColorId = '" + searchBody.colorId.ToString()+"'";

                            if ((searchBody.sizeId != null) && (searchBody.sizeId != "") && (searchBody.sizeId != "0"))
                            {
                                if ((searchBody.branchId != null) && (searchBody.branchId != "") && (searchBody.branchId != "0"))
                                    cmd.CommandText = "SELECT * FROM QMTS WHERE (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "') AND SizeId = '" + searchBody.sizeId.ToString() + "' AND ColorId = '" + searchBody.colorId.ToString() + "' AND BranchId = '"+searchBody.branchId.ToString()+"'";
                                else
                                    cmd.CommandText = "SELECT * FROM QMTS WHERE (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "') AND SizeId = '" + searchBody.sizeId.ToString() + "' AND ColorId = '" + searchBody.colorId.ToString()+"'";
                            }
                        }
                        else
                        {
                            if ((searchBody.sizeId != null) && (searchBody.sizeId != "") && (searchBody.sizeId != "0"))
                            {
                                if ((searchBody.branchId != null) && (searchBody.branchId != "") && (searchBody.branchId != "0"))
                                    cmd.CommandText = "SELECT * FROM QMTS WHERE (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "') AND SizeId = '" + searchBody.sizeId.ToString() + "' AND BranchId = '"+searchBody.branchId.ToString()+"'";
                                else
                                    cmd.CommandText = "SELECT * FROM QMTS WHERE (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "') AND SizeId = '" + searchBody.sizeId.ToString()+"'";
                            }
                        }
                    }
                    List<Item> items = new List<Item>();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        

                        while (reader.Read())
                        {
                            code = reader["Code"].ToString();
                            computerNo = reader["ComputerNo"].ToString();
                            barcode = reader["Barcode"].ToString();
                            color = reader["ColorName"].ToString();
                            size = reader["SizeName"].ToString();
                            price = reader["EndUser"].ToString();
                            sale = reader["Sale"].ToString();
                            localcost = reader["Localcost"].ToString();
                            branch = reader["BranchName"].ToString();
                            qty = Convert.ToInt32(reader["Qty"]);
                            branchId = reader["BranchId"].ToString();

                            Item item = new Item();

                            if (qty > 0)
                            {
                                item.Code = code;
                                item.ComputerNo = computerNo;
                                item.Price = price;
                                item.Cost = localcost;
                                item.Qty = qty.ToString();
                                item.Barcode = barcode;
                                item.Branch = branch;
                                item.Color = color;
                                item.Size = size;
                                item.Sale = sale;
                                item.BranchId = branchId;

                                items.Add(item);
                            }

                        }



                    }

                    reader.Close();

                    cmd.CommandText = "SELECT distinct ColorId, ColorName FROM QMTD WHERE (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "')";
                    /*if (searchBody.colorId > 0)
                    {
                        cmd.CommandText = "SELECT distinct ColorId, ColorName FROM QMTD WHERE " + searchField + " Like '" + searchBody.searchText + "%' AND ColorId = '" + searchBody.colorId.ToString()+"'";
                        if (searchBody.sizeId > 0)
                            cmd.CommandText = "SELECT distinct ColorId, ColorName FROM QMTD WHERE " + searchField + " Like '" + searchBody.searchText + "%' AND SizeId = '" + searchBody.sizeId.ToString() + "' AND ColorId = '" + searchBody.colorId.ToString()+"'";
                    }
                    else
                    {
                        if (searchBody.sizeId > 0)
                            cmd.CommandText = "SELECT distinct ColorId, ColorName FROM QMTS WHERE " + searchField + " Like '" + searchBody.searchText + "%' AND SizeId = '" + searchBody.sizeId.ToString()+"'";
                    }*/


                    List<ItemColor> colors = new List<ItemColor>();
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        

                        string colorId = "";
                        string colorName = "";

                        while (reader.Read())
                        {
                            colorId = reader["ColorId"].ToString();
                            colorName = reader["ColorName"].ToString();

                            ItemColor colorR = new ItemColor();

                            colorR.Id = colorId;
                            colorR.Name = colorName;
                            colors.Add(colorR);

                        }

                    }

                    reader.Close();



                    cmd.CommandText = "SELECT distinct SizeId, SizeName FROM QMTD WHERE (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "')";
                    /*if (searchBody.colorId > 0)
                    {
                        cmd.CommandText = "SELECT distinct SizeId, SizeName FROM QMTS WHERE " + searchField + " Like '" + searchBody.searchText + "%' AND ColorId = '" + searchBody.colorId.ToString()+"'";
                        if (searchBody.sizeId > 0)
                            cmd.CommandText = "SELECT distinct SizeId, SizeName FROM QMTS WHERE " + searchField + " Like '" + searchBody.searchText + "%' AND SizeId = '" + searchBody.sizeId.ToString() + "' AND ColorId = '" + searchBody.colorId.ToString()+"'";
                    }
                    else
                    {
                        if (searchBody.sizeId > 0)
                            cmd.CommandText = "SELECT distinct SizeId, SizeName FROM QMTS WHERE " + searchField + " Like '" + searchBody.searchText + "%' AND SizeId = '" + searchBody.sizeId.ToString()+"'";
                    }*/


                    List<ItemSize> sizes = new List<ItemSize>();
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {

                        string sizeId = "";
                        string sizeName = "";

                        while (reader.Read())
                        {
                            sizeId = reader["SizeId"].ToString();
                            sizeName = reader["SizeName"].ToString();

                            ItemSize sizeR = new ItemSize();

                            sizeR.Id = sizeId;
                            sizeR.Name = sizeName;
                            sizes.Add(sizeR);

                        }

                    }


                    reader.Close();

                    cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    if (searchBody.searchText != "")
                    {

                        if ((searchBody.branchId != null) && (searchBody.branchId != "") && (searchBody.branchId != "0"))
                            cmd.CommandText = "SELECT Distinct BranchId, BranchName, BranchEName FROM QMTS WHERE QTY > 0 AND (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "') AND BranchId = '" + searchBody.branchId.ToString() + "'";
                        else
                            cmd.CommandText = "SELECT Distinct BranchId, BranchName, BranchEName FROM QMTS WHERE QTY > 0 AND (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "')";

                        if ((searchBody.colorId != null) && (searchBody.colorId != "") && (searchBody.colorId != "0"))
                        {
                            if ((searchBody.branchId != null) && (searchBody.branchId != "") && (searchBody.branchId != "0"))
                                cmd.CommandText = "SELECT Distinct BranchId, BranchName, BranchEName FROM QMTS WHERE QTY > 0 AND (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "') AND ColorId = '" + searchBody.colorId.ToString() + "' AND BranchId = '" + searchBody.branchId.ToString() + "'";
                            else
                                cmd.CommandText = "SELECT Distinct BranchId, BranchName, BranchEName FROM QMTS WHERE QTY > 0 AND (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "') AND ColorId = '" + searchBody.colorId.ToString() + "'";

                            if ((searchBody.sizeId != null) && (searchBody.sizeId != "") && (searchBody.sizeId != "0"))
                            {
                                if ((searchBody.branchId != null) && (searchBody.branchId != "") && (searchBody.branchId != "0"))
                                    cmd.CommandText = "SELECT Distinct BranchId, BranchName, BranchENameFROM QMTS WHERE QTY > 0 AND (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "') AND SizeId = '" + searchBody.sizeId.ToString() + "' AND ColorId = '" + searchBody.colorId.ToString() + "' AND BranchId = '" + searchBody.branchId.ToString() + "'";
                                else
                                    cmd.CommandText = "SELECT Distinct BranchId, BranchName, BranchEName FROM QMTS WHERE QTY > 0 AND (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "') AND SizeId = '" + searchBody.sizeId.ToString() + "' AND ColorId = '" + searchBody.colorId.ToString() + "'";
                            }
                        }
                        else
                        {
                            if ((searchBody.sizeId != null) && (searchBody.sizeId != "") && (searchBody.sizeId != "0"))
                            {
                                if ((searchBody.branchId != null) && (searchBody.branchId != "") && (searchBody.branchId != "0"))
                                    cmd.CommandText = "SELECT Distinct BranchId, BranchName, BranchEName FROM QMTS WHERE QTY > 0 AND (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "') AND SizeId = '" + searchBody.sizeId.ToString() + "' AND BranchId = '" + searchBody.branchId.ToString() + "'";
                                else
                                    cmd.CommandText = "SELECT Distinct BranchId, BranchName, BranchEName FROM QMTS WHERE QTY > 0 AND (Barcode = '" + searchBody.searchText + "' OR ComputerNo = '" + searchBody.searchText + "') AND SizeId = '" + searchBody.sizeId.ToString() + "'";
                            }
                        }
                    }

                    List<Branch> branches = new List<Branch>();
                    reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {

                        string xbranchId = "";
                        string xbranchName = "";
                        string xbranchEName = "";

                        while (reader.Read())
                        {
                            xbranchId = reader["BranchId"].ToString();
                            xbranchName = reader["BranchName"].ToString();
                            xbranchEName = reader["BranchEName"].ToString();

                            Branch br = new Branch();

                            br.ID = xbranchId;
                            br.ArabicName = xbranchName;
                            br.EnglishName = xbranchEName;
                            branches.Add(br);

                        }

                    }

                    var res = new SearchResult();

                    res.Items = items;
                    res.Colors = colors;
                    res.Sizes = sizes;
                    res.Branches = branches;

                    return Ok(res);
                    
                }
            }
            catch (Exception e)
            {
                return Ok(null);
            }
        }


        [HttpPost("GetSales")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSales([FromBody] DateFilter dateFilter)
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    List<Sales> sales = new List<Sales>();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText =
" select branches.BranchId, branches.BranchName, " +
" ceiling(sum(case when bld.IsOutput = 1 then(bld.FinalPrice * bld.qty) else ((bld.FinalPrice * bld.qty) * (-1)) end)) as TotalValue, " +
" sum(case when bld.IsOutput = 1 then bld.qty else (bld.qty * (-1)) end) as TotalQty from bli inner join bsr on bli.blid = bsr.blid " +
" inner " +
" join Branches on Branches.BranchID = bsr.Branch " +
" inner " +
" join bld on bld.blid = bli.blid and bld.BLNo = bli.BLNo " +
//" where bsr.bstid = 5 and branches.branchId not in (4,21,24,14,33,32) " +
" where bsr.bstid = 5 " +
" and bli.BLDate between '" +dateFilter.fromDate+" 00:00:00.000' and '"+dateFilter.toDate+" 23:59:59.000' " +
" group by bsr.blid, Branches.BranchName, Branches.BranchId ";

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Sales sale = new Sales();
                            sale.BranchId = reader["BranchId"].ToString();
                            sale.BranchName = reader["BranchName"].ToString();
                            sale.TotalValue = reader["TotalValue"].ToString();
                            sale.TotalQty = reader["TotalQty"].ToString();
                            sales.Add(sale);
                        }
                    }

                    reader.Close();
                    return Ok(sales.OrderByDescending((x) => Convert.ToDouble(x.TotalValue)).ToList());
                }
            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }


        [HttpGet("GetReceivedTransfers")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReceivedTransfers(string branchId)
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    List<TransferMaster> transfersToReceive = new List<TransferMaster>();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = "SELECT * FROM QDTI WHERE ReceivingBy IS NULL and To_Br = '"+branchId+"'";
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            TransferMaster transferMaster = new TransferMaster();
                            transferMaster.Blno = reader["BLNO"].ToString();
                            transferMaster.FromBranchName = reader["From_BranchName"].ToString();
                            transferMaster.TotalQty = Convert.ToInt32(reader["TotalQty"]);
                            transferMaster.Date = Convert.ToDateTime(reader["BLDate"]);
                            transfersToReceive.Add(transferMaster);
                        }
                    }

                    reader.Close();
                    return Ok(transfersToReceive);
                }
            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }

        [HttpGet("GetTransferDetails")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTransferDetails(string BLNO)
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    List<TransferDetail> transferDetails = new List<TransferDetail>();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = "SELECT * FROM QDTD WHERE BLNO = '" + BLNO + "'";
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            TransferDetail transferDetail = new TransferDetail();
                            transferDetail.ComputerNo = reader["ComputerNo"].ToString();
                            transferDetail.Barcode = reader["Barcode"].ToString();
                            transferDetail.Color = reader["ColorName"].ToString();
                            transferDetail.Size = reader["SizeName"].ToString();
                            transferDetail.Qty = Convert.ToInt32(reader["Qty"]);
                            transferDetails.Add(transferDetail);
                        }
                    }

                    reader.Close();
                    return Ok(transferDetails);
                }
            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }

        [HttpPost("ReceiveTransfer")]
        [AllowAnonymous]
        public async Task<IActionResult> ReceiveTransfer([FromBody] ReceiveTransferDto receiveTransferDto)
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandTimeout = 60000;
                    cmd.CommandText = "UPDATE DTI SET RDate = '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + ".000" + "', QPost = 1, ReceivingBy = '" + receiveTransferDto.userId + "', PostedBy = '" + receiveTransferDto.userName + "' WHERE BLNO = '" + receiveTransferDto.blno + "'";
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    return Ok(true);
                }
            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }


        [HttpPost("SaveSettings")]
        [AllowAnonymous]
        public async Task<IActionResult> SaveSettings([FromBody] DeviceSettings deviceSettings)
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandTimeout = 60000;
                    cmd.CommandText = "DELETE AppSettings WHERE DeviceId = '" + deviceSettings.DeviceId + "'; " +
                        " INSERT INTO AppSettings(DeviceId, BranchId, CanShowSales, CanMakeTransfers, CanReceiveTransfers, " +
                        "UserId, Username, BranchesInSearch, BranchesToTransferTo, IsAdmin, Fullname) " +
                        "VALUES('" +
                        deviceSettings.DeviceId + "','" +
                        deviceSettings.BranchId + "'," +
                        (deviceSettings.CanShowSales ? "1" : "0") + "," +
                        (deviceSettings.CanMakeTransfers ? "1" : "0") + "," +
                        (deviceSettings.CanReceiveTransfers ? "1" : "0") + "," +
                        "'" + deviceSettings.UserId + "'," +
                        "'" + deviceSettings.Username + "'," +
                        "'" + string.Join(",", deviceSettings.BranchesInSearch) + "'," +
                        "'" + string.Join(",", deviceSettings.BranchesToTransferTo) + "'," +
                        (deviceSettings.IsAdmin ? "1" : "0") + "," +
                        "'" + deviceSettings.Fullname + "'); ";
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    return Ok(true);
                }

            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }


        [HttpGet("GetSettings")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSettings(string deviceId)
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandTimeout = 60000;
                    cmd.CommandText = "SELECT * FROM AppSettings WHERE DeviceId = '" + deviceId + "'";

                    DeviceSettings deviceSettings = new DeviceSettings();

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        deviceSettings.DeviceId = reader["DeviceId"].ToString();
                        deviceSettings.BranchId = reader["BranchId"].ToString();
                        deviceSettings.UserId = reader["UserId"].ToString();
                        deviceSettings.Username = reader["Username"].ToString();
                        deviceSettings.IsAdmin = Convert.ToBoolean(reader["IsAdmin"]);
                        deviceSettings.CanShowSales = Convert.ToBoolean(reader["CanShowSales"]);
                        deviceSettings.CanMakeTransfers = Convert.ToBoolean(reader["CanMakeTransfers"]);
                        deviceSettings.CanReceiveTransfers = Convert.ToBoolean(reader["CanReceiveTransfers"]);
                        deviceSettings.BranchesToTransferTo = reader["BranchesToTransferTo"].ToString().Split(new char[] { ',' }).ToList();
                        deviceSettings.BranchesInSearch = reader["BranchesInSearch"].ToString().Split(new char[] { ',' }).ToList();
                        deviceSettings.Fullname = reader["Fullname"].ToString();
                    }
                    conn.Close();
                    return Ok(deviceSettings);
                }

            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }


        [HttpGet("GetDevices")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDevices()
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandTimeout = 60000;
                    cmd.CommandText = "SELECT * FROM AppSettings";

                    List<DeviceSettings> devices = new List<DeviceSettings>();
                    DeviceSettings deviceSettings = new DeviceSettings();

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            deviceSettings = new DeviceSettings();
                            deviceSettings.DeviceId = reader["DeviceId"].ToString();
                            deviceSettings.BranchId = reader["BranchId"].ToString();
                            deviceSettings.UserId = reader["UserId"].ToString();
                            deviceSettings.Username = reader["Username"].ToString();
                            deviceSettings.IsAdmin = Convert.ToBoolean(reader["IsAdmin"]);
                            deviceSettings.CanShowSales = Convert.ToBoolean(reader["CanShowSales"]);
                            deviceSettings.CanMakeTransfers = Convert.ToBoolean(reader["CanMakeTransfers"]);
                            deviceSettings.CanReceiveTransfers = Convert.ToBoolean(reader["CanReceiveTransfers"]);
                            deviceSettings.BranchesToTransferTo = reader["BranchesToTransferTo"].ToString().Split(new char[] { ',' }).ToList();
                            deviceSettings.BranchesInSearch = reader["BranchesInSearch"].ToString().Split(new char[] { ',' }).ToList();
                            deviceSettings.Fullname = reader["Fullname"].ToString();
                            devices.Add(deviceSettings);
                        }
                    }
                    conn.Close();
                    return Ok(devices);
                }

            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }

        [HttpGet("GetOceanUsers")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOceanUsers()
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandTimeout = 60000;
                    cmd.CommandText = "SELECT * FROM USI";

                    List<User> users = new List<User>();
                    User user = new User();

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            user = new User();
                            user.UsId = reader["UsId"].ToString();
                            user.UsName = reader["UsName"].ToString();
                            users.Add(user);
                        }
                    }
                    conn.Close();
                    return Ok(users);
                }

            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }


        [HttpPost("SaveDevices")]
        [AllowAnonymous]
        public async Task<IActionResult> SaveDevices([FromBody] List<DeviceSettings> devicesSettings)
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    devicesSettings.ForEach((d) => {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = conn;
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.CommandTimeout = 60000;
                        cmd.CommandText = "DELETE AppSettings WHERE DeviceId = '" + d.DeviceId + "'; " +
                            " INSERT INTO AppSettings(DeviceId, BranchId, CanShowSales, CanMakeTransfers, CanReceiveTransfers, " +
                            "UserId, Username, BranchesInSearch, BranchesToTransferTo, IsAdmin, Fullname) " +
                            "VALUES('" +
                            d.DeviceId + "','" +
                            d.BranchId + "'," +
                            (d.CanShowSales ? "1" : "0") + "," +
                            (d.CanMakeTransfers ? "1" : "0") + "," +
                            (d.CanReceiveTransfers ? "1" : "0") + "," +
                            "'" + d.UserId + "'," +
                            "'" + d.Username + "'," +
                            "'" + string.Join(",", d.BranchesInSearch) + "'," +
                            "'" + string.Join(",", d.BranchesToTransferTo) + "'," +
                            (d.IsAdmin ? "1" : "0") + "," +
                            "'" + d.Fullname + "'); ";
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    });

                    return Ok(true);
                }

            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }


        [HttpGet("DeleteDevice")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteSettings(string deviceId)
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandTimeout = 60000;
                    cmd.CommandText = "DELETE AppSettings WHERE DeviceId = '" + deviceId + "'";
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    return Ok(true);
                }

            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }


        [HttpGet("FindCustomer")]
        [AllowAnonymous]
        public async Task<IActionResult> FindCustomer(string mobileNumber)
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandTimeout = 60000;
                    cmd.CommandText = "SELECT TOP 1 * FROM POSCustomers WHERE MobilePhone = '" + mobileNumber + "'";

                    Customer customer = new Customer();

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        customer.Id = reader["CustomerId"].ToString();
                        customer.Name = reader["CustomerName"].ToString();
                        customer.MobileNumber = reader["MobilePhone"].ToString();
                        customer.BranchId = reader["RegBr"].ToString();
                    }
                    conn.Close();
                    return Ok(customer);
                }

            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }

        [HttpPost("AddCustomer")]
        [AllowAnonymous]
        public async Task<IActionResult> AddCustomer([FromBody] Customer customer)
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandTimeout = 60000;
                    cmd.CommandText = "INSERT INTO POSCustomers(MobilePhone, CustomerName, RegBr) VAUES('" + customer.MobileNumber + "', '" + customer.Name + "','" + customer.BranchId + "')";
                    cmd.ExecuteNonQuery();
                    
                    return Ok(true);
                }

            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }

        [HttpGet("GetSalesInvoices/{branchId}/{fromDate}/{toDate}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSalesInvoices(string branchId, string fromDate, string toDate)
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    List<Invoice> invoices = new List<Invoice>();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = "SELECT * FROM QBLI WHERE BranchId = '"+branchId+"' AND BLDATE BETWEEN '"+fromDate+"' AND '"+toDate+"'";
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Invoice invoice = new Invoice();
                            invoice.Blno = reader["BLNO"].ToString();
                            invoice.TotalQty = Convert.ToInt32(reader["TotalQty"]);
                            invoice.Date = Convert.ToDateTime(reader["BLDate"]);
                            invoice.FinalValue = Convert.ToDouble(reader["FinalValue"]);
                            invoices.Add(invoice);
                        }
                    }

                    reader.Close();
                    return Ok(invoices);
                }
            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }


        [HttpGet("GetSalesInvoiceDetails")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSalesInvoiceDetails(string BLNO)
        {
            try
            {
                using (SqlConnection conn = DataConnection.GetDataConnection())
                {
                    List<InvoiceDetail> invoiceDetails = new List<InvoiceDetail>();
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = "SELECT * FROM QBLD WHERE BLNO = '" + BLNO + "'";
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            InvoiceDetail invoiceDetail = new InvoiceDetail();
                            invoiceDetail.ComputerNo = reader["ComputerNo"].ToString();
                            invoiceDetail.Barcode = reader["Barcode"].ToString();
                            invoiceDetail.Color = reader["ColorName"].ToString();
                            invoiceDetail.Size = reader["SizeName"].ToString();
                            invoiceDetail.Qty = Convert.ToInt32(reader["Qty"]);
                            invoiceDetail.Price = Convert.ToDouble(reader["Price"]);
                            invoiceDetails.Add(invoiceDetail);
                        }
                    }

                    reader.Close();
                    return Ok(invoiceDetails);
                }
            }
            catch (Exception e)
            {
                return Ok(false);
            }

        }

    }
}
