using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BasicWebServerLib;
using BasicWebServerLib.Events;
using BasicWebServerLib.HttpCommon;
using Excel = Microsoft.Office.Interop.Excel;

namespace AmortizationApp {
  public class Handlers {
    private readonly string _serverBaseFolder;
    private readonly Helpers _helpers;
    private readonly Dictionary<string, Action> _actions;
    private Dictionary<string, object> _requestDictionary;
    private HttpConnectionDetails _httpDetails;
    
    public Handlers(string serverBaseFolder) {
      _serverBaseFolder = serverBaseFolder;
      _helpers = new Helpers();
      
      string[] months = new string[]{"Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"};
      
      _actions = new Dictionary<string, Action>() {
        {"calculateLoan", () => {
          try {
            int? monthlyExtraPayment = null;
            string onetimeExtraPaymentDate = null;
            int? onetimeExtraPayment = null;
            
            int startMonthIdx = Convert.ToInt32(_requestDictionary["start_month_idx"]);
            int startYear = Convert.ToInt32(_requestDictionary["start_year"]);
            int amount = Convert.ToInt32(_requestDictionary["amount"]);
            decimal yearlyInterest = Convert.ToDecimal(_requestDictionary["yearly_interest"]);
            int loanTerm = Convert.ToInt32(_requestDictionary["loan_term"]);

            if(_requestDictionary["monthly_extra_payment"] != null) {
              monthlyExtraPayment = Convert.ToInt32(_requestDictionary["monthly_extra_payment"]);
            }
            if(_requestDictionary["onetime_extra_payment_date"] != null) {
              onetimeExtraPaymentDate = (string)_requestDictionary["onetime_extra_payment_date"];
            }
            if(_requestDictionary["onetime_extra_payment"] != null) {
              onetimeExtraPayment = Convert.ToInt32(_requestDictionary["onetime_extra_payment"]);
            }
         

            decimal monthlyInterest = (yearlyInterest * (decimal)0.01) / (decimal)12.0;
            decimal val = (decimal)Math.Pow((double)(monthlyInterest + (decimal)1.0), loanTerm);
            decimal? bankPay = (amount * (monthlyInterest * val) / (val - (decimal)1.0));
            
            List<string> dateList = new List<string>();
            List<decimal?> bankpayList = new List<decimal?>();
            List<decimal?> extrapayList = new List<decimal?>();
            List<decimal?> principalList = new List<decimal?>();
            List<decimal?> interestList = new List<decimal?>();
            List<decimal?> totalInterestList = new List<decimal?>();
            List<decimal?> balanceList = new List<decimal?>();

            decimal? balance = amount;
            decimal? extrapay;
            decimal? interest;
            decimal? principal;
            decimal? totalInterest = 0;
            string payDate = null;

            for(int i = 0; i <= loanTerm - 1; i++) {
              int monthIdx = (startMonthIdx + i) % 12;
              payDate = months[monthIdx] + " " + startYear;
              dateList.Add(payDate);
              if(monthIdx == 11) {
                startYear += 1;
              }

              if(onetimeExtraPaymentDate != null && onetimeExtraPayment != null &&
                 payDate == onetimeExtraPaymentDate) {
                extrapay = onetimeExtraPayment;
              }else if(monthlyExtraPayment != null) {
                extrapay = monthlyExtraPayment;
              } else {
                extrapay = 0;
              }
              
              interest = balance * monthlyInterest;
              principal = bankPay - interest;
              balance = balance - principal - extrapay;
              totalInterest += interest;
              totalInterestList.Add(totalInterest);
              
              if(balance < 0) {
                principal = balanceList[i-1];
                balance = 0;
                extrapay = 0;
                bankPay = interest + principal;
                bankpayList.Add(bankPay);
                extrapayList.Add(extrapay);
                principalList.Add(principal);
                interestList.Add(interest);
                balanceList.Add(balance);
                break;
              } else {
                bankpayList.Add(bankPay);
                extrapayList.Add(extrapay);
                principalList.Add(principal);
                interestList.Add(interest);
                balanceList.Add(balance);
              }        
            }

            Dictionary<string, object> responseDict = new Dictionary<string, object>() {
              {"total_interest",totalInterest},
              {"payoff_date",payDate},
              {"date_list",dateList},
              {"bankpay_list",bankpayList},
              {"principal_list",principalList},
              {"interest_list",interestList},
              {"total_interest_list",totalInterestList},
              {"extrapay_list",extrapayList},
              {"balance_list",balanceList}
            };
            string responseStr = _helpers.DictionaryToJson(responseDict);
            _helpers.SendHttpTextResponse(_httpDetails.Response, responseStr);

          } catch(Exception ex) {
            _helpers.SendHttpResponse(500, ex.Message,new byte[0],"text/html","Calculate Loan", _httpDetails.Response);
          }  
        }},
        {"createExcel", () => {
          try {
            int amount = Convert.ToInt32(_requestDictionary["amount"]);
            decimal yearlyInterest = Convert.ToDecimal(_requestDictionary["yearly_interest"]);
            int loanTerm = Convert.ToInt32(_requestDictionary["loan_term"]);
            
            ArrayList dateList = _helpers.JArrayToArrayList(_requestDictionary["date_list"]);
            ArrayList bankpayList = _helpers.JArrayToArrayList(_requestDictionary["bankpay_list"]);
            ArrayList principalList = _helpers.JArrayToArrayList(_requestDictionary["principal_list"]);
            ArrayList interestList = _helpers.JArrayToArrayList(_requestDictionary["interest_list"]);
            ArrayList totalInterestList = _helpers.JArrayToArrayList(_requestDictionary["total_interest_list"]);
            ArrayList extrapayList = _helpers.JArrayToArrayList(_requestDictionary["extrapay_list"]);
            ArrayList balanceList = _helpers.JArrayToArrayList(_requestDictionary["balance_list"]);
              
            //Create Excel App
            Excel.Application excelApp = new Excel.Application();
            excelApp.Visible = true;

            Excel.Workbook workbook = excelApp.Workbooks.Add(Missing.Value);
            Excel.Worksheet loanSheet = (Excel.Worksheet)workbook.ActiveSheet;
            loanSheet.Name = "Amortization";

            string paydateCol = ColumnNumberToName(2);
            string bankpayCol = ColumnNumberToName(3);
            string principalCol = ColumnNumberToName(4);
            string interestCol = ColumnNumberToName(5);
            string totalInterestCol = ColumnNumberToName(6);
            string extrapayCol = ColumnNumberToName(7);
            string balanceCol = ColumnNumberToName(8);
            
            //create expense title cell
            loanSheet.Cells[2, 2] = "Amortization for $" + amount + " at " + yearlyInterest + "% over " + loanTerm + " months";
            loanSheet.get_Range(paydateCol+2, bankpayCol+2).Font.Bold = true;
            loanSheet.get_Range(paydateCol+2, bankpayCol+2).Font.Size = 22;
            
            //set column widths for headings, value cells
            loanSheet.get_Range(paydateCol + 3, paydateCol + 300).ColumnWidth = 12;
            loanSheet.get_Range(bankpayCol + 3, bankpayCol + 300).ColumnWidth = 12;
            loanSheet.get_Range(principalCol + 3, principalCol + 300).ColumnWidth = 12;
            loanSheet.get_Range(interestCol + 3, interestCol + 300).ColumnWidth = 12;
            loanSheet.get_Range(totalInterestCol + 3, totalInterestCol + 300).ColumnWidth = 16;
            loanSheet.get_Range(extrapayCol + 3, extrapayCol + 300).ColumnWidth = 12;
            loanSheet.get_Range(balanceCol + 3, balanceCol + 300).ColumnWidth = 16;
            
            //set column alignments
            loanSheet.get_Range(paydateCol + 3, paydateCol + 300).HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
            loanSheet.get_Range(bankpayCol + 3, bankpayCol + 300).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            loanSheet.get_Range(principalCol + 3, principalCol + 300).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            loanSheet.get_Range(interestCol + 3, interestCol + 300).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            loanSheet.get_Range(totalInterestCol + 3, totalInterestCol + 300).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            loanSheet.get_Range(extrapayCol + 3, extrapayCol + 300).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            loanSheet.get_Range(balanceCol + 3, balanceCol + 300).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            
            //label heading cells
            loanSheet.get_Range(paydateCol + 6, balanceCol + 6).Font.Bold = true;
            loanSheet.get_Range(paydateCol + 6, balanceCol + 6).Font.Size = 14;
            loanSheet.Cells[6, 2] = "Date";
            loanSheet.Cells[6, 3] = "Payment";
            loanSheet.Cells[6, 4] = "Principal";
            loanSheet.Cells[6, 5] = "Interest";
            loanSheet.Cells[6, 6] = "Total Interest";
            loanSheet.Cells[6, 7] = "Extra Pay";
            loanSheet.Cells[6, 8] = "Balance";
            
            //assign values
            for(int i=0; i<dateList.Count; i++) {
              var rowI = i + 7;
              loanSheet.Cells[7 + i, 2] = "01-" + dateList[i];
              loanSheet.get_Range(bankpayCol + rowI,principalCol + rowI).Formula = "=Fixed(" + bankpayList[i] + ",2,TRUE)";
              loanSheet.get_Range(principalCol + rowI,interestCol + rowI).Formula = "=Fixed(" + principalList[i] + ",2,TRUE)";
              loanSheet.get_Range(interestCol + rowI,totalInterestCol + rowI).Formula = "=Fixed(" + interestList[i] + ",2,TRUE)";
              loanSheet.get_Range(totalInterestCol +rowI,extrapayCol + rowI).Formula = "=Fixed(" + totalInterestList[i] + ",2,TRUE)";
              loanSheet.get_Range(extrapayCol + rowI,balanceCol + rowI).Formula = "=Fixed(" + extrapayList[i] + ",2,TRUE)";
              loanSheet.get_Range(balanceCol + rowI,balanceCol + rowI).Formula = "=Fixed(" + balanceList[i] + ",2,TRUE)";
            }
            
            //save changes and close workbook
            workbook.Close(true,Type.Missing,Type.Missing);
            //close Excel server
            excelApp.Quit();
            
            _helpers.SendHttpTextResponse(_httpDetails.Response, "Completed Excel Workbook");

          } catch(Exception ex) {
            _helpers.SendHttpResponse(500, ex.Message,new byte[0],"text/html","Create Excel", _httpDetails.Response);
          }
        }}
      };
    }
    
    public void StartServer() {
      try {
        BasicWebServer basicServer = new BasicWebServer(baseFolderPath: _serverBaseFolder, tcpPort: null,
          httpPrefix: "http://localhost:8088/");
        basicServer.HttpRequestChanged += HttpRequestChanged;

        basicServer.Start();
      } catch(Exception ex) {
        Console.WriteLine("Startup Error:" + ex.Message);
      }
    }
    
    public void HttpRequestChanged(object sender, EventArgs args) {
      HttpRequestEventArgs httpArgs = (HttpRequestEventArgs)args;
      _httpDetails = httpArgs.Details;
      string body = (string)httpArgs.Body;
      _requestDictionary = _helpers.JsonToDictionary(body);
      
      if(_httpDetails.HttpPath == "loan") {
        _actions[(string)_requestDictionary["action"]]();
      }
    }
    
    // Return the column name for this column number.
    private string ColumnNumberToName(int colNum) {
      // See if it's out of bounds.
      if (colNum < 1) return "A";

      // Calculate the letters.
      string result = "";
      while (colNum > 0) {
        // Get the least significant digit.
        colNum -= 1;
        int digit = colNum % 26;

        // Convert the digit into a letter.
        result = (char)((int)'A' + digit) + result;

        colNum = colNum / 26;
      }
      return result;
    }
  }
}