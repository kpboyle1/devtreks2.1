﻿<?xml version="1.0" encoding="UTF-8" ?>
<!-- Author: www.devtreks.org, 2013, January -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:y0="urn:DevTreks-support-schemas:Investment"
	xmlns:DisplayDevPacks="urn:displaydevpacks">
	<xsl:output method="xml" indent="yes" omit-xml-declaration="yes" encoding="UTF-8" />
	<!-- pass in params -->
	<!-- what action is being taken by the server -->
	<xsl:param name="serverActionType" />
	<!-- what other action is being taken by the server -->
	<xsl:param name="serverSubActionType" />
	<!-- is the member viewing this uri the owner? -->
	<xsl:param name="isURIOwningClub" />
	<!-- which node to start with? -->
	<xsl:param name="nodeName" />
	<!-- which view to use? -->
	<xsl:param name="viewEditType" />
	<!-- is this a coordinator? -->
	<xsl:param name="memberRole" />
	<!-- what is the current uri? -->
	<xsl:param name="selectedFileURIPattern" />
	<!-- the addin being used -->
	<xsl:param name="calcDocURI" />
	<!-- the node being calculated-->
	<xsl:param name="docToCalcNodeName" />
	<!-- standard params used with calcs and custom docs -->
	<xsl:param name="calcParams" />
	<!-- what is the name of the node to be selected? -->
	<xsl:param name="selectionsNodeNeededName" />
	<!-- which network is this doc from? -->
	<xsl:param name="networkId" />
	<!-- what is the start row? -->
	<xsl:param name="startRow" />
	<!-- what is the end row? -->
	<xsl:param name="endRow" />
	<!-- what are the pagination properties ? -->
	<xsl:param name="pageParams" />
	<!-- what is the guide's email? -->
	<xsl:param name="clubEmail" />
	<!-- what is the current serviceid? -->
	<xsl:param name="contenturipattern" />
	<!-- path to resources -->
	<xsl:param name="fullFilePath" />
	<!-- init html -->
	<xsl:template match="@*|/|node()" />
	<xsl:template match="/">
		<xsl:apply-templates select="root" />
	</xsl:template>
	<xsl:template match="root">
		<div id="mainwrapper">
			<table class="data" cellpadding="6" cellspacing="1" border="0">
				<tbody>
					<xsl:apply-templates select="servicebase" />
					<xsl:apply-templates select="investmentgroup" />
					<tr id="footer">
						<td scope="row" colspan="10">
							<a id="aFeedback" name="Feedback">
								<xsl:attribute name="href">mailto:<xsl:value-of select="$clubEmail" />?subject=<xsl:value-of select="$selectedFileURIPattern" /></xsl:attribute>
								Feedback About <xsl:value-of select="$selectedFileURIPattern" />
							</a>
						</td>
					</tr>
				</tbody>
			</table>
		</div>
	</xsl:template>
	<xsl:template match="servicebase">
		<tr>
			<th colspan="10">
				Service: <xsl:value-of select="@Name" />
			</th>
		</tr>
		<xsl:apply-templates select="investmentgroup">
			<xsl:sort select="@Name"/>
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="investmentgroup">
		<tr>
			<th scope="col" colspan="10">
				Investment Group
			</th>
		</tr>
		<tr>
			<td colspan="10">
				<strong><xsl:value-of select="@Name" /> </strong>
			</td>
		</tr>
		<xsl:variable name="calculatorid"><xsl:value-of select="@CalculatorId"/></xsl:variable>
		<xsl:apply-templates select="root/linkedview[@Id=$calculatorid]">
			<xsl:with-param name="localName"><xsl:value-of select="local-name()" /></xsl:with-param>
		</xsl:apply-templates>
    <xsl:apply-templates select="investment">
			<xsl:sort select="@Date"/>
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="investment">
		<tr>
			<th scope="col" colspan="10">
				Investment
			</th>
		</tr>
		<tr>
			<td colspan="10">
				<strong><xsl:value-of select="@Name" /> </strong>
			</td>
		</tr>
		<xsl:variable name="calculatorid"><xsl:value-of select="@CalculatorId"/></xsl:variable>
		<xsl:apply-templates select="root/linkedview[@Id=$calculatorid]">
			<xsl:with-param name="localName"><xsl:value-of select="local-name()" /></xsl:with-param>
		</xsl:apply-templates>
    <xsl:apply-templates select="investmenttimeperiod">
			<xsl:sort select="@Date"/>
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="investmenttimeperiod">
		<tr>
			<th scope="col" colspan="10"><strong>Time Period</strong></th>
		</tr>
		<tr>
			<td scope="row" colspan="10">
				<strong><xsl:value-of select="@PracticeName" /></strong>
			</td>
		</tr>
			<tr>
				<th scope="col" colspan="10">Outcomes</th>
			</tr>
		  <xsl:apply-templates select="investmentoutcomes" />
		<tr>
			<th scope="col" colspan="10">Components</th>
		</tr>
		 <tr>
			<td>
				<strong>
					Market Value
				</strong>
			</td>
			<td>
				<strong>
					Salvage Value
				</strong>
			</td>
			<td>
				<strong>
					Cap Recov Cost
				</strong>
			</td>
			<td>
				<strong>
					THI Cost
				</strong>
			</td>
			<td>
				<strong>
					Starting Hrs
				</strong>
			</td>
			<td>
				<strong>
					Planned Use Hrs
				</strong>
			</td>
			<td>
				<strong>
					Useful Life Hrs
				</strong>
			</td>
			<td>
				<strong>
					Horsepower
				</strong>
			</td>
			<td>
				<strong>
					Speed
				</strong>
			</td>
			<td>
				<strong>
					Width
				</strong>
			</td>
		</tr>
		<tr>
			<td>
				<strong>
					Fuel Amount
				</strong>
			</td>
			<td>
				<strong>
					Fuel Price
				</strong>
			</td>
			<td>
				<strong>
					Fuel Cost
				</strong>
			</td>
			<td>
				<strong>
					Labor Amount
				</strong>
			</td>
			<td>
				<strong>
					Labor Price
				</strong>
			</td>
			<td>
				<strong>
					Labor Cost
				</strong>
			</td>
			<td>
				<strong>
					Lube Oil Amounts
				</strong>
			</td>
			<td>
				<strong>
					Lube Oil Price
				</strong>
			</td>
			<td>
				<strong>
					Lube Oil Cost
				</strong>
			</td>
			<td>
				<strong>
					Repair Cost
				</strong>
			</td>
		</tr>
    <tr>
			<td>
				<strong>
					Equiv PTO HP
				</strong>
			</td>
			<td>
				<strong>
					Field Efficiency
				</strong>
			</td>
      <td>
        <strong>
          Amount
        </strong>
      </td>
			<td>
				<strong>
					Operating Cost
				</strong>
			</td>
			<td>
				<strong>
					Alloc OH Cost
				</strong>
			</td>
			<td>
			</td>
			<td>
			</td>
			<td>
			</td>
			<td>
			</td>
			<td>
			</td>
		</tr>
		<xsl:apply-templates select="investmentcomponents" />
		<tr>
			<th scope="col" colspan="10"><strong>Time Period Totals</strong></th>
		</tr>
		<xsl:variable name="calculatorid"><xsl:value-of select="@CalculatorId"/></xsl:variable>
		<xsl:apply-templates select="root/linkedview[@Id=$calculatorid]">
			<xsl:with-param name="localName"><xsl:value-of select="local-name()" /></xsl:with-param>
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="investmentoutcomes">
		<xsl:apply-templates select="investmentoutcome">
			<xsl:sort select="@Date"/>
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="investmentoutcome">
		<tr>
			<th scope="col" colspan="10"><strong>Outcome</strong></th>
		</tr>
		<tr>
			<td scope="row" colspan="10">
				<strong><xsl:value-of select="@Name" /></strong>
			</td>
		</tr>
		<xsl:variable name="calculatorid"><xsl:value-of select="@CalculatorId"/></xsl:variable>
		<xsl:apply-templates select="root/linkedview[@Id=$calculatorid]">
			<xsl:with-param name="localName"><xsl:value-of select="local-name()" /></xsl:with-param>
		</xsl:apply-templates>
    <xsl:apply-templates select="investmentoutput">
    </xsl:apply-templates>
	</xsl:template>
	<xsl:template match="investmentoutput">
		<!--this release does not include resource stock output calculators-->
	</xsl:template>
	<xsl:template match="investmentcomponents">
		<xsl:apply-templates select="investmentcomponent">
			<xsl:sort select="@Date"/>
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="investmentcomponent">
    <tr>
			<th scope="col" colspan="10"><strong>Component</strong></th>
		</tr>
		<tr>
			<td scope="row" colspan="10">
				<strong><xsl:value-of select="@Name" /></strong>
			</td>
		</tr>
		<xsl:variable name="calculatorid"><xsl:value-of select="@CalculatorId"/></xsl:variable>
		<xsl:apply-templates select="root/linkedview[@Id=$calculatorid]">
			<xsl:with-param name="localName"><xsl:value-of select="local-name()" /></xsl:with-param>
		</xsl:apply-templates>
    <xsl:apply-templates select="investmentinput">
    </xsl:apply-templates>
	</xsl:template>
	<xsl:template match="investmentinput">
		<tr>
			<td scope="row" colspan="10">
				<strong>Input : <xsl:value-of select="@Name" /></strong>
			</td>
		</tr>
		<xsl:variable name="calculatorid"><xsl:value-of select="@CalculatorId"/></xsl:variable>
		<xsl:apply-templates select="root/linkedview[@Id=$calculatorid]">
			<xsl:with-param name="localName"><xsl:value-of select="local-name()" /></xsl:with-param>
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="root/linkedview">
		<xsl:param name="localName" />
		<xsl:if test="($localName != 'investmentinput')">
      <tr>
		      <td scope="col" colspan="10"><strong>Costs</strong></td>
	    </tr>
			<tr>
				<td>
					<xsl:value-of select="@TMarketValue" />
				</td>
				<td>
					<xsl:value-of select="@TSalvageValue" />
				</td>
				<td>
					<xsl:value-of select="@TCapitalRecoveryCost" />
				</td>
				<td>
					<xsl:value-of select="@TTaxesHousingInsuranceCost" />
				</td>
				<td>
					<xsl:value-of select="@TStartingHrs" />
				</td>
				<td>
					<xsl:value-of select="@TPlannedUseHrs" />
				</td>
				<td>
					<xsl:value-of select="@TUsefulLifeHrs" />
				</td>
				<td>
					<xsl:value-of select="@THP" />
				</td>
				<td>
					<xsl:value-of select="@TSpeed" />
				</td>
				<td>
					<xsl:value-of select="@TWidth" />
				</td>
			</tr>
			<tr>
				<td>
					<xsl:value-of select="@TFuelAmount" />
				</td>
				<td>
					<xsl:value-of select="@TFuelPrice" />
				</td>
				<td>
					<xsl:value-of select="@TFuelCost" />
				</td>
				<td>
					<xsl:value-of select="@TLaborAmount" />
				</td>
				<td>
					<xsl:value-of select="@TLaborPrice" />
				</td>
				<td>
					<xsl:value-of select="@TLaborCost" />
				</td>
				<td>
					<xsl:value-of select="@TLubeOilAmount" />
				</td>
				<td>
					<xsl:value-of select="@TLubeOilPrice" />
				</td>
				<td>
					<xsl:value-of select="@TLubeOilCost" />
				</td>
				<td>
					<xsl:value-of select="@TRepairCost" />
				</td>
			</tr>
      <tr>
        <td>
          <xsl:value-of select="@THPPTOEquiv" />
        </td>
        <td>
          <xsl:value-of select="@TFieldEffTypical" />
        </td>
        <td>
          n.a.
        </td>
        <td>
          <xsl:value-of select="@TOC" />
        </td>
        <td>
          <xsl:value-of select="@TAOH" />
        </td>
        <td>
        </td>
        <td>
        </td>
        <td>
        </td>
        <td>
        </td>
        <td>
        </td>
      </tr>
		</xsl:if>
		<xsl:if test="($localName = 'investmentinput')">
      <tr>
		      <td scope="col" colspan="10"><strong>Costs</strong></td>
	    </tr>
			<tr>
				<td>
					<xsl:value-of select="@MarketValue" />
				</td>
				<td>
					<xsl:value-of select="@SalvageValue" />
				</td>
				<td>
					<xsl:value-of select="@CapitalRecoveryCost" />
				</td>
				<td>
					<xsl:value-of select="@TaxesHousingInsuranceCost" />
				</td>
				<td>
					<xsl:value-of select="@StartingHrs" />
				</td>
				<td>
					<xsl:value-of select="@PlannedUseHrs" />
				</td>
				<td>
					<xsl:value-of select="@UsefulLifeHrs" />
				</td>
				<td>
					<xsl:value-of select="@HP" />
				</td>
				<td>
					<xsl:value-of select="@FieldSpeedTypical" />
				</td>
				<td>
					<xsl:value-of select="@Width" />
				</td>
			</tr>
			<tr>
				<td>
					<xsl:value-of select="@FuelAmount" />
				</td>
				<td>
					<xsl:value-of select="@FuelPrice" />
				</td>
				<td>
					<xsl:value-of select="@FuelCost" />
				</td>
				<td>
					<xsl:value-of select="@LaborAmount" />
				</td>
				<td>
					<xsl:value-of select="@LaborPrice" />
				</td>
				<td>
					<xsl:value-of select="@LaborCost" />
				</td>
				<td>
					<xsl:value-of select="@LubeOilAmount" />
				</td>
				<td>
					<xsl:value-of select="@LubeOilPrice" />
				</td>
				<td>
					<xsl:value-of select="@LubeOilCost" />
				</td>
				<td>
					<xsl:value-of select="@RepairCost" />
				</td>
			</tr>
      <tr>
        <td>
          <xsl:value-of select="@HPPTOEquiv" />
        </td>
        <td>
          <xsl:value-of select="@FieldEffTypical" />
        </td>
        <td>
          <xsl:value-of select="@ServiceCapacity"/>
        </td>
        <td>
          <xsl:value-of select="@TOC" />
        </td>
        <td>
          <xsl:value-of select="@TAOH" />
        </td>
        <td>
        </td>
        <td>
        </td>
        <td>
        </td>
        <td>
        </td>
        <td>
        </td>
      </tr>
		</xsl:if>
	</xsl:template>
</xsl:stylesheet>