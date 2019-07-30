## AmortizationApp

**AmortizationApp** is a locally hosted web browser application that determines the monthly payments on a fixed loan as well as  an amortization schedule for a given payment period.  The application accepts extra payments for investigating various payback scenarios.  Also if Microsoft Office is installed, the application can optionally create an Excel spreadsheet showing the complete schedule.  **AmortizationApp** was inspired by the online loan calculator at [Bankrate](https://www.bankrate.com/calculators/mortgages/loan-calculator.aspx).

The application is Windows based and assumes that Microsoft .NET Framework 4.7.1 is installed. Download and extract the `AmortizationApp-master` zip file from the [Amortization repository](https://github.com/deandevl/Amortization.git) . Under the `Installation` directory locate and run the `setup.exe` install file.  

A desktop and start menu short cuts are provided to start the local html server (i.e. the `AmortizationApp.exe` executable).  From either a Chrome or Firefox browser enter `localhost:8088` in the url address box .  The **AmortizationApp**'s main page will then be rendered.

To use simply enter the loan amount, term in months, the yearly interest rate,  the month/year the loan starts, and any extra monthly payments at the top of the main page.  Click the `Calculate` button and a scrollable table will be crated showing the complete schedule with the date,  bank payment, interest, principal, accumulating interest total, extra payment for  each month, and the monthly balance.  Click the `Create Excel Sheet` button to produce an Excel sheet similar to the displayed table.

As an example, enter the following:

​	`Loan Amount` ---  5000

​	`Yearly Interest Rate` ---  4.5

​	`No. of Payments` ---  60

​	`Month/Year of Payment Start` ---  (Enter the current month and year)

Click the 'Calculate' button and results will show the following:

​	`Monthly Payments`  ---   93.22

​	`Total Principal Paid`  ---  5000

​	`Total Interest Paid` ---   592.91

​	`Estimated Payoff Date`  ---  (month and year 60 months from above payment start)

Click the `Extra Payments` arrow and enter `100` for the `Extra Payment(s)` to your monthly payment.  Click the 'Calculate' button again and results will show the following:

​	`Monthly Payments`  ---   93.22

​	`Total Principal Paid`  ---  5000

​	`Total Interest Paid` ---   269.47 with 28 months of payments





