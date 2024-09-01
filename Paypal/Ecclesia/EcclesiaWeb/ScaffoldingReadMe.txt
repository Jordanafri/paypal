Support for ASP.NET Core Identity was added to your project.

For setup and configuration information, see https://go.microsoft.com/fwlink/?linkid=2116645.
 // paypal.Buttons({
    //     async createOrder() {
    //         const response = await fetch("@Url.ActionLink("CreateOrder", "Checkout")", {
    //             method: "POST",
    //             headers: {
    //                 "Content-Type": "application/json",
    //             },
    //             body: JSON.stringify({
    //                 // cart: [{
    //                 //     sku: "YOUR_PRODUCT_STOCK_KEEPING_UNIT",
    //                 //     quantity: "YOUR_PRODUCT_QUANTITY",
    //                 // }]
    //                 amount: document.getElementById("totalAmount").value
    //             })
    //         });

    //         const order = await response.json();
    //         console.log(order);
    //         return order.data.Id;
    //     },

    //     async onApprove(data) {
    //         // Capture the funds from the transaction.
    //         const response = await fetch("@Url.ActionLink("CompleteOrder", "Checkout")", {
    //             method: "POST",
    //             headers: {
    //                 "Content-Type": "application/json",
    //             },
    //             body: JSON.stringify({
    //                 orderID: data.orderID
    //             })
    //         })

    //         const details = await response.text(); ///was .json() but now .text()

    //         if (details == "success") {
    //             document.getElementById("notification-container").innerHTML =
    //                 `<div class='alert alert-success alert-dismissible fade show' role='alert'>
    //                         <strong> The order is created successfully </strong>
    //                         <button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>
    //                      </div>
    //                     `;
    //         }
    //         else {
    //             document.getElementById("notification-container").innerHTML =
    //                 `<div class='alert alert-danger alert-dismissible fade show' role='alert'>
    //                             <strong> Failed to create order! </strong>
    //                             <button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>
    //                          </div>
    //                         `;
    //         }
    //     },

    //     //for alert
    //     onCancel(data) {
    //         document.getElementById("notification-container").innerHTML =
    //             `<div class='alert alert-danger alert-dismissible fade show' role='alert'>
    //                     <strong> Payment Canceled! </strong>
    //                     <button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>
    //                  </div>
    //                 `;

    //     },
    //     onError(err) {
    //         document.getElementById("notification-container").innerHTML =
    //             `<div class='alert alert-danger alert-dismissible fade show' role='alert'>
    //                     <strong> an error occured! </strong>
    //                     <button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>
    //                  </div>
    //                 `;

    //     }

    // }).render('#paypal-button-container');


    public void ConfigureServices(IServiceCollection services)
{
    services.Configure<PayPalConfig>(Configuration.GetSection("PayPal"));
    services.AddTransient<IPayPalService, PayPalService>();
    // ...
}var payPalConfig = new Dictionary<string, string>
{
    { "clientId", builder.Configuration.GetSection("PayPal:ClientId").Value },
    { "clientSecret", builder.Configuration.GetSection("PayPal:ClientSecret").Value },
    { "mode", builder.Configuration.GetSection("PayPal:Mode").Value } // sandbox or live
};
SD.PayPalClientId = payPalConfig["clientId"];
SD.PayPalClientSecret = payPalConfig["clientSecret"];