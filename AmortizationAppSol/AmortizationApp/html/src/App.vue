<template>
  <div class="amortize">
    <h2 class="amortize_heading">Amortization Schedule with Updated Extra Payments</h2>
    <section class="amortize_terms">
      <input-comp
          heading="Loan Amount($)"
          header_position="below"
          placeholder="Enter loan amt"
          input_size="10"
          :input_value="amount"
          :single_border="input_single_border"
          :css_variables="input_css_variables"
          v-on:input_comp_value_changed="value => {this.amount = this.round(value)}">
      </input-comp>
      <input-comp
          heading="Yearly Interest Rate"
          header_position="below"
          placeholder="Enter interest"
          input_size="10"
          :input_value="yearly_interest"
          :single_border="input_single_border"
          :css_variables="input_css_variables"
          v-on:input_comp_value_changed="value => {this.yearly_interest = this.round(value)}">
      </input-comp>
      <input-comp
          heading="No. of Payments"
          header_position="below"
          placeholder="Enter term"
          input_size="10"
          :input_value="loan_term"
          :single_border="input_single_border"
          :css_variables="input_css_variables"
          v-on:input_comp_value_changed="value => {this.loan_term = this.round(value)}">
      </input-comp>
      <input-comp
          heading="Month/Year of Payment Start"
          header_position="below"
          input_type="month"
          :input_value="start_payment_date"
          :single_border="input_single_border"
          :css_variables="input_css_variables"
          v-on:input_comp_value_changed="value => {this.start_payment_date = value}">
      </input-comp>
    </section>
    <section class="amortize_extra_payment_sec">
      <collapse-comp
          heading="Extra Payments"
          :blur_panel="collapse_blur_panel"
          :css_variables="collapse_css_variables">
        <section class="amortize_extra_payment">
          <section class="amortize_extra_type_1">
            <input-comp
                heading="Extra Payment($)"
                header_position="below"
                placeholder="Enter payment"
                input_size="13"
                :input_value="onetime_extra_payment"
                :single_border="input_single_border"
                :css_variables="input_css_variables"
                v-on:input_comp_value_changed="value => {this.onetime_extra_payment = this.round(value)}">
            </input-comp>
            <span>as a one-time payment in</span>
            <select-comp
                heading="Month/Year"
                placeholder="Select a date"
                :items="date_list"
                :select_value="onetime_extra_payment_date"
                :single_border="select_single_border"
                :css_variables="select_css_variables"
                v-on:select_comp_value_changed="value => {this.onetime_extra_payment_date = value}">
            </select-comp>
          </section>
          <section class="amortize_extra_type_2">
            <input-comp
                heading="Extra Payment($)"
                header_position="below"
                input_size="13"
                placeholder="Enter payment"
                :input_value="monthly_extra_payment"
                :single_border="input_single_border"
                :css_variables="input_css_variables"
                v-on:input_comp_value_changed="value => {this.monthly_extra_payment = this.round(value)}">
            </input-comp>
            <span>to your monthly payment</span>
          </section>
        </section>
      </collapse-comp>
    </section>
    <section class="amortize_buttons">
      <button-comp
          v-on:button_comp_clicked="calculate_loan">Calculate
      </button-comp>
      <button-comp
          v-on:button_comp_clicked="create_excel">Create Excel Sheet
      </button-comp>
    </section>
    <section class="amortize_summary">
      <label-comp
          heading="Monthly Payments"
          header_position="below"
          :value=bankpay
          :css_variables="label_css_variables">
      </label-comp>
      <label-comp
          heading="Total Principal Paid"
          header_position="below"
          :value=amount
          :css_variables="label_css_variables">
      </label-comp>
      <label-comp
          heading="Total Interest Paid"
          header_position="below"
          :value=total_interest
          :css_variables="label_css_variables">
      </label-comp>
      <label-comp
          heading="Estimated Payoff Date"
          header_position="below"
          :value=payoff_date
          :css_variables="label_css_variables">
      </label-comp>
    </section>
    <section class="amortize_table">
      <table-comp
          :rows="table_rows"
          :headings="table_headings"
          :column_widths="table_col_widths"
          :css_variables="table_css_variables">
      </table-comp>
    </section>
    <section class="amortize_statusSec">{{status_content}}</section>
  </div>
</template>

<script>
  import Vue from 'vue';
  import ButtonComp from 'buttoncomp';
  import InputComp from 'inputcomp';
  import SelectComp from 'selectcomp';
  import CollapseComp from 'collapsecomp';
  import LabelComp from 'labelcomp';
  import TableComp from 'tablecomp';
  
  export default {
    name: "App",
    data: function() {
      return {
        localhost: 'http://localhost:8088/loan',
        status_content: "Status",
        loan_bus: new Vue(),

        amount: null,
        yearly_interest: null,
        loan_term: null,
        payments: null,
        bankpay: 0,

        date_list: null,
        bankpay_list: null,
        extrapay_list: null,
        principal_list: null,
        interest_list: null,
        total_interest_list: null,
        balance_list: null,

        total_interest: 0,
        start_payment_date: null,
        onetime_extra_payment_date: null,
        onetime_extra_payment: null,
        monthly_extra_payment: null,
        months: [
          'Jan',
          'Feb',
          'Mar',
          'Apr',
          'May',
          'Jun',
          'Jul',
          'Aug',
          'Sep',
          'Oct',
          'Nov',
          'Dec'
        ],
        payoff_date: null,

        input_single_border: true,
        input_css_variables: {
          input_comp_heading_color: 'white',
          input_comp_input_color: 'white',
          input_comp_input_border_color: 'white',
          input_comp_input_placeholder_color: 'white',
          input_comp_input_focus_background: 'black'
        },

        label_css_variables: {
          label_comp_heading_color: 'white',
          label_comp_value_color: 'gold'
        },

        collapse_blur_panel: false,
        collapse_css_variables: {
          collapse_comp_heading_color: 'white',
          collapse_comp_icon_color: 'white'
        },

        select_single_border: true,
        select_css_variables: {
          select_comp_color: 'white',
          select_comp_border_color: 'white',
          select_comp_heading_color: 'white',
          select_comp_items_panel_color: 'white',
          select_comp_items_panel_background: 'transparent'
        },

        table_rows: null,
        table_headings: ['Date','Bank Pay','Principal','Interest','Total Interest','Extra Pay','Balance'],
        table_col_widths: [100,80,80,80,80,80,80,80],

        table_css_variables: {
          table_comp_tbody_height: '14rem',
          table_comp_title_color: 'white',
          table_comp_thead_color: 'white',
          table_comp_thead_background: 'black',
          table_comp_thead_border_bottom: '2px solid white',
          table_comp_row_color: 'gray',
          table_comp_row_selected_color: 'gold',
          table_comp_row_border_bottom: '1px solid white',
          table_comp_row_odd_background: 'black',
          table_comp_row_even_background: 'black',
          table_comp_cell_font_size: '16px'
        }
      }
    },
    components: {
      ButtonComp,
      InputComp,
      SelectComp,
      CollapseComp,
      LabelComp,
      TableComp
    },
    methods: {
      calculate_loan: function(){
        let parts = this.start_payment_date.split('-');
        let start_year = parseInt(parts[0]);
        const start_month_idx = parseInt(parts[1]) - 1;

        const url=this.localhost;
        const request_data= {
          action: 'calculateLoan',
          start_month_idx: start_month_idx,
          start_year: start_year,
          amount: this.amount,
          yearly_interest: this.yearly_interest,
          loan_term: this.loan_term,
          monthly_extra_payment: this.monthly_extra_payment,
          onetime_extra_payment_date: this.onetime_extra_payment_date,
          onetime_extra_payment: this.onetime_extra_payment
        };
        const request_data_str=JSON.stringify(request_data);
        const config={
          method: 'POST',
          mode: 'cors',
          body: request_data_str,
          headers: new Headers({
            'Content-Type': 'application/json',
            'Content-Length': request_data_str.length
          })
        };
        fetch(url, config).then(response => {
          if(response.ok) {
            return response.text();
          }
          throw new Error(response.statusText);
        }).then(resp_str => {
          const resp_dict=JSON.parse(resp_str);
          this.total_interest = resp_dict['total_interest'].toFixed(2);
          this.payoff_date = resp_dict['payoff_date'];
          this.date_list = resp_dict['date_list'];
          this.bankpay_list = resp_dict['bankpay_list'];
          this.principal_list = resp_dict['principal_list'];
          this.interest_list = resp_dict['interest_list'];
          this.total_interest_list = resp_dict['total_interest_list'];
          this.extrapay_list = resp_dict['extrapay_list'];
          this.balance_list = resp_dict['balance_list'];

          this.monthly_extra_payment = null;
          this.onetime_extra_payment = null;
          this.bankpay = this.bankpay_list[0].toFixed(2);
          this.update_table();

          this.loan_bus.$emit('status_changed','Loan calculated for $' + this.amount);
        }).catch(error => {
          this.loan_bus.$emit('status_changed', `Calculate loan error: ${error.message}`)
        });
      },
      update_table: function(){
        this.table_rows = [];
        for(let i=0; i<this.date_list.length; i++){  
          const row = [
            [this.date_list[i],''],
            [(+this.bankpay_list[i]).toFixed(2),''],
            [(+this.principal_list[i]).toFixed(2),''],
            [(+this.interest_list[i]).toFixed(2),''],
            [(+this.total_interest_list[i]).toFixed(2),''],
            [(+this.extrapay_list[i]).toFixed(2),''],
            [(+this.balance_list[i]).toFixed(2),'']];
          this.table_rows.push(row);
        }
      },
      create_excel: function(){
        const url=this.localhost;
        const request_data={
          action: 'createExcel',
          amount: this.amount,
          yearly_interest: this.yearly_interest,
          loan_term: this.loan_term,
          date_list: this.date_list,
          bankpay_list: this.bankpay_list,
          principal_list: this.principal_list,
          interest_list: this.interest_list,
          total_interest_list: this.total_interest_list,
          extrapay_list: this.extrapay_list,
          balance_list: this.balance_list
        };
        const request_data_str=JSON.stringify(request_data);
        const config={
          method: 'POST',
          mode: 'cors',
          body: request_data_str,
          headers: new Headers({
            'Content-Type': 'application/json',
            'Content-Length': request_data_str.length
          })
        };
        fetch(url, config).then(response => {
          if(response.ok) {
            return response.text();
          }
          throw new Error(response.statusText);
        }).then(resp_str => {
          this.loan_bus.$emit('status_changed',resp_str);
        }).catch(error => {
          this.loan_bus.$emit('status_changed', `Create Excel sheet error: ${error.message}`)
        });
      },
      round: function(value_str) {
        const value = parseFloat(value_str);
        return Math.round(value*100)/100;
      }
    },
    mounted(){
      //set up 'status_changed' event
      this.loan_bus.$on('status_changed', (message) => {
        this.status_content=message;
      });
      this.table_rows = [
        [['',''],[0.0,''],[0.0,''],[0.0,''],[0.0,''],[0.0,''],[0.0,'']]
      ]
    }
  }
</script>

<style lang="less">
  .amortize {
    display: flex;
    flex-direction: column;
    font-family: Verdana,sans-serif;
    padding: 2rem;
    background-color: black;
    color: white;
    width: 100%;
    height: 100%;

    &_heading {
      font-size: 2.25rem;
    }

    &_terms {
      display: flex;
      flex-direction: row;
      justify-content: space-between;
      align-self: center;
      width: 850px;
    }

    &_extra_payment_sec {
      margin: 2rem 0 0 0;
    }

    &_extra_payment {
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      height: 150px;
    }

    &_extra_type_1 {
      display: flex;
      flex-direction: row;
      justify-content: space-between;
      align-items: center;
      width: 40rem;
    }

    &_extra_type_2 {
      display: flex;
      flex-direction: row;
      justify-content: space-between;
      align-items: center;
      width: 26rem;
    }

    &_buttons {
      display: flex;
      flex-direction: row;
      justify-content: space-between;
      width: 20rem;
      align-self: center;
      margin-top: 3rem;
    }

    &_summary {
      display: flex;
      flex-direction: row;
      justify-content: space-between;
      min-width: 54rem;
      align-self: center;
      margin: 2rem 0 2rem 0;
    }

    &_table {
      align-self: center;
    }

    &_statusSec {
      margin-top: 1rem;
      font-size: 1rem;
      min-width: 54rem;
      padding-left: 2rem;
      color: white;
    }

    h2 {
      align-self: center;
    }
  }
</style>