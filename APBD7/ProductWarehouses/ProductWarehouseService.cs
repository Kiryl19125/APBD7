using System.Data.SqlClient;

namespace APBD7.Models;

public interface IProductWarehouseService
{
    Task<bool> IsProductWarehouseExist(int productID, int warehouseID);

    Task<bool> IsOrderInOrderTable(ProductWarehouse productWarehouse);
    Task<bool> IsOrderComplete(ProductWarehouse productWarehouse);

    void UpdateFullfilledAt(ProductWarehouse productWarehouse);

    Task<int> InsertIntoProductWarehouse(ProductWarehouse productWarehouse);
}

public class ProductWarehouseService : IProductWarehouseService
{
    private IConfiguration _configuration;

    public ProductWarehouseService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> IsProductWarehouseExist(int productID, int warehouseID)
    {
        using (var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
        {
            var countProductCommand = connection.CreateCommand();
            countProductCommand.CommandText = "select count(*) from Product where IdProduct = @productId";
            countProductCommand.Parameters.AddWithValue("@productId", productID);
            await connection.OpenAsync();
            int productCount = Convert.ToInt32(await countProductCommand.ExecuteScalarAsync());

            var countWarehouseCommand = connection.CreateCommand();
            countWarehouseCommand.CommandText = """
                                                  select count(*)
                                                  from Warehouse
                                                  where IdWarehouse = @warehouseId
                                                """;
            countWarehouseCommand.Parameters.AddWithValue("@warehouseId", warehouseID);
            // await connection.OpenAsync();
            int warehouseCount = Convert.ToInt32(await countProductCommand.ExecuteScalarAsync());

            return warehouseCount > 0 && productCount > 0;
        }
    }

    public async Task<bool> IsOrderInOrderTable(ProductWarehouse productWarehouse)
    {
        using (var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
        {
            var commend = connection.CreateCommand();
            commend.CommandText = """
                                  select CreatedAt
                                  from [Order]
                                     where IdProduct = @productId and Amount = @productAmount;
                                  """;
            commend.Parameters.AddWithValue("@productId", productWarehouse.idProduct);
            commend.Parameters.AddWithValue("@productAmount", productWarehouse.amount);
            DateTime timeFromTable = (DateTime)await commend.ExecuteScalarAsync();
            if (timeFromTable < productWarehouse.createdAt)
            {
                return true;
            }

            return false;
        }
    }

    public async Task<bool> IsOrderComplete(ProductWarehouse productWarehouse)
    {
        using (var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
        {
            var commend = connection.CreateCommand();
            commend.CommandText = """
                                  select count(*) from Product_Warehouse where IdProduct = @productID;
                                  """;
            commend.Parameters.AddWithValue("@productID", productWarehouse.idProduct);
            int count = Convert.ToInt32(await commend.ExecuteScalarAsync());

            if (count > 0)
            {
                return true;
            }

            return false;
        }
    }

    public async void UpdateFullfilledAt(ProductWarehouse productWarehouse)
    {
        using (var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
        {
            var commend = connection.CreateCommand();
            commend.CommandText = """
                                  update [Order]
                                      set FulfilledAt = GETDATE()
                                      where IdProduct = @productID and Amount = @amount;
                                  """;
            commend.Parameters.AddWithValue("@productID", productWarehouse.idProduct);
            commend.Parameters.AddWithValue("@amount", productWarehouse.amount);
            await commend.ExecuteScalarAsync();
        }
    }

    public async Task<int> InsertIntoProductWarehouse(ProductWarehouse productWarehouse)
    {
        using (var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
        {
            var getOrderIdCommand = connection.CreateCommand();
            getOrderIdCommand.CommandText = """
                                            select IdOrder
                                            from [Order]
                                            where IdProduct = @productID;
                                            """;
            getOrderIdCommand.Parameters.AddWithValue("@productID", productWarehouse.idProduct);
            int orderID = (int)await getOrderIdCommand.ExecuteScalarAsync();

            var getProductPriceCommand = connection.CreateCommand();
            getProductPriceCommand.CommandText = """
                                                 select Price from Product where IdProduct = @productID;
                                                 """;
            getProductPriceCommand.Parameters.AddWithValue("@productID", productWarehouse.idProduct);
            float productPrice = (float)await getOrderIdCommand.ExecuteScalarAsync();


            var commend = connection.CreateCommand();
            commend.CommandText = """
                                  INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                                  VALUES (@warehouseID, @productID, @orderID, @amount, @price, GETDATE());
                                  """;
            commend.Parameters.AddWithValue("@warehouseID", productWarehouse.idWarehouse);
            commend.Parameters.AddWithValue("@productID", productWarehouse.idProduct);
            commend.Parameters.AddWithValue("@orderID", orderID);
            commend.Parameters.AddWithValue("@amount", productWarehouse.amount);
            commend.Parameters.AddWithValue("@price", productWarehouse.amount * productPrice);
            await commend.ExecuteScalarAsync();

            var getProductWareHouseKey = connection.CreateCommand();
            getProductWareHouseKey.CommandText = """
                                                 select IdProductWarehouse
                                                 from Product_Warehouse
                                                 where IdProduct = @productID and IdOrder = @orderID and IdWarehouse = @warehouseID;
                                                 """;
            getProductWareHouseKey.Parameters.AddWithValue("@productID", productWarehouse.idProduct);
            getProductWareHouseKey.Parameters.AddWithValue("@orderID", orderID);
            getProductWareHouseKey.Parameters.AddWithValue("@warehouseID", productWarehouse.idWarehouse);

            return (int)await getProductWareHouseKey.ExecuteScalarAsync();
        }
    }
}