﻿<?xml version="1.0" encoding="UTF-8" ?>
<!-- Author: www.devtreks.org, 2013, December -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:y0="urn:DevTreks-support-schemas:Budget"
	xmlns:DisplayComps="urn:displaycomps">
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
		<div id="modEdits_divEditsDoc">
      <xsl:apply-templates select="servicebase" />
			<xsl:apply-templates select="budgetgroup" />
			<div>
				<a id="aFeedback" name="Feedback">
					<xsl:attribute name="href">mailto:<xsl:value-of select="$clubEmail" />?subject=<xsl:value-of select="$selectedFileURIPattern" /></xsl:attribute>
					Feedback About <xsl:value-of select="$selectedFileURIPattern" />
				</a>
      </div>
		</div>
	</xsl:template>
	<xsl:template match="servicebase">
		<h4 class="ui-bar-b">
			Service: <xsl:value-of select="@Name" />
		</h4>
		<xsl:apply-templates select="budgetgroup">
			<xsl:sort select="@Name"/>
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="budgetgroup">
    <h4>
      <strong>Budget Group</strong> : <xsl:value-of select="@Name" /> ; <xsl:value-of select="@Num_0" />
    </h4>
		<xsl:apply-templates select="root/linkedview[@AnalyzerType='npvstat1']">
			<xsl:with-param name="localName"><xsl:value-of select="local-name()" /></xsl:with-param>
		</xsl:apply-templates>
    <xsl:apply-templates select="budget">
			<xsl:sort select="@Date"/>
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="budget">
		<xsl:variable name="fullcolcount" select="@Files + 1"/>
    <h4 class="ui-bar-b">
      Budget Name and Label
    </h4>
		<xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
		<xsl:for-each select="@*">
			<xsl:variable name="att_name" select="name()"/>
			<xsl:variable name="att_value" select="."/>
			<xsl:value-of select="DisplayComps:printValue('Name_', $att_name, $att_value)"/> 
		</xsl:for-each>
    <div class="ui-grid-a">
			<xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
    </div>
    <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
		<xsl:for-each select="@*">
			<xsl:variable name="att_name" select="name()"/>
			<xsl:variable name="att_value" select="."/>
			<xsl:value-of select="DisplayComps:printValue('Num_', $att_name, $att_value)"/> 
		</xsl:for-each>
    <div class="ui-grid-a">
			<xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
    </div>
		<xsl:apply-templates select="root/linkedview[@AnalyzerType='npvstat1']">
			<xsl:with-param name="localName"><xsl:value-of select="local-name()" /></xsl:with-param>
		</xsl:apply-templates>
    <xsl:apply-templates select="budgettimeperiod">
			<xsl:sort select="@Date"/>
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="budgettimeperiod">
    <xsl:variable name="fullcolcount" select="@Files + 1"/>
    <h4 class="ui-bar-b">
      Time Period Name and Label
    </h4>
		<xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
		<xsl:for-each select="@*">
			<xsl:variable name="att_name" select="name()"/>
			<xsl:variable name="att_value" select="."/>
			<xsl:value-of select="DisplayComps:printValue('Name_', $att_name, $att_value)"/> 
		</xsl:for-each>
    <div class="ui-grid-a">
			<xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
    </div>
    <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
		<xsl:for-each select="@*">
			<xsl:variable name="att_name" select="name()"/>
			<xsl:variable name="att_value" select="."/>
			<xsl:value-of select="DisplayComps:printValue('Num_', $att_name, $att_value)"/> 
		</xsl:for-each>
    <div class="ui-grid-a">
			<xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
    </div>
    <xsl:apply-templates select="root/linkedview[@AnalyzerType='npvstat1']">
			<xsl:with-param name="localName"><xsl:value-of select="local-name()" /></xsl:with-param>
		</xsl:apply-templates>
    <div>
      <strong>Outcomes</strong>
    </div>
		<xsl:apply-templates select="budgetoutcomes" />
    <div>
      <strong>Operations</strong>
    </div>
		<xsl:apply-templates select="budgetoperations" />
	</xsl:template>
	<xsl:template match="budgetoutcomes">
		<xsl:apply-templates select="budgetoutcome">
			<xsl:sort select="@Date"/>
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="budgetoutcome">
		<xsl:variable name="fullcolcount" select="@Files + 1"/>
    <h4 class="ui-bar-b">
      Outcome Name and Label
    </h4>
		<xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
		<xsl:for-each select="@*">
			<xsl:variable name="att_name" select="name()"/>
			<xsl:variable name="att_value" select="."/>
			<xsl:value-of select="DisplayComps:printValue('Name_', $att_name, $att_value)"/> 
		</xsl:for-each>
    <div class="ui-grid-a">
			<xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
    </div>
    <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
		<xsl:for-each select="@*">
			<xsl:variable name="att_name" select="name()"/>
			<xsl:variable name="att_value" select="."/>
			<xsl:value-of select="DisplayComps:printValue('Num_', $att_name, $att_value)"/> 
		</xsl:for-each>
    <div class="ui-grid-a">
			<xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
    </div>
		<xsl:apply-templates select="root/linkedview[@AnalyzerType='npvstat1']">
			<xsl:with-param name="localName"><xsl:value-of select="local-name()" /></xsl:with-param>
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="budgetoperations">
		<xsl:apply-templates select="budgetoperation">
			<xsl:sort select="@Date"/>
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="budgetoperation">
		<xsl:variable name="fullcolcount" select="@Files + 1"/>
    <h4 class="ui-bar-b">
      Operation Name and Label
    </h4>
		<xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
		<xsl:for-each select="@*">
			<xsl:variable name="att_name" select="name()"/>
			<xsl:variable name="att_value" select="."/>
			<xsl:value-of select="DisplayComps:printValue('Name_', $att_name, $att_value)"/> 
		</xsl:for-each>
    <div class="ui-grid-a">
			<xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
    </div>
    <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
		<xsl:for-each select="@*">
			<xsl:variable name="att_name" select="name()"/>
			<xsl:variable name="att_value" select="."/>
			<xsl:value-of select="DisplayComps:printValue('Num_', $att_name, $att_value)"/> 
		</xsl:for-each>
    <div class="ui-grid-a">
			<xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
    </div>
		<xsl:apply-templates select="root/linkedview[@AnalyzerType='npvstat1']">
			<xsl:with-param name="localName"><xsl:value-of select="local-name()" /></xsl:with-param>
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="root/linkedview">
		<xsl:param name="localName" />
    <xsl:variable name="fullcolcount" select="@Files + 1"/>
   <xsl:if test="($localName != 'budgetoperation' and $localName != 'budgetinput')">
      <div data-role="collapsible" data-collapsed="true" data-theme="b" data-content-theme="d" >
        <h4 class="ui-bar-b">
          Benefits
        </h4>
        <h4 class="ui-bar-b">
          Benefit Observations 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:if test="starts-with($att_name, 'TBenefitN_')">
					  <xsl:value-of select="DisplayComps:printValue('TBenefitN_', $att_name, $att_value)"/> 
				  </xsl:if>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Total Benefit
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMR_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Benefit Mean 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMR_MEAN_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Benefit Median 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMR_MED_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Benefit Variance 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMR_VAR2_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Benefit Std Dev 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMR_SD_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          R Incent Total 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMRINCENT_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          R Incent Mean 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMRINCENT_MEAN_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          R Incent Median 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMRINCENT_MED_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          R Incent Variance 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMRINCENT_VAR2_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          R Incent Std Dev 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMRINCENT_SD_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Output Q Total 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TRAmount_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Output Q Mean 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TRAmount_MEAN_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Output Q Median 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TRAmount_MED_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Output Q Variance 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TRAmount_VAR2_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Output Q Std Dev 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TRAmount_SD_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Output P Total 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TRPrice_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Output P Mean 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TRPrice_MEAN_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Output P Median 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TRPrice_MED_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Output P Variance 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TRPrice_VAR2_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Output P Std Dev 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TRPrice_SD_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
      </div>
    </xsl:if>
    <xsl:if test="($localName != 'budgetoutcome' and $localName != 'budgetoutput')">
      <div data-role="collapsible" data-collapsed="true" data-theme="b" data-content-theme="d" >
        <h4 class="ui-bar-b">
          Costs
        </h4>
        <h4 class="ui-bar-b">
          Cost Observations 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:if test="starts-with($att_name, 'TCostN_')">
					  <xsl:value-of select="DisplayComps:printValue('TCostN_', $att_name, $att_value)"/> 
				  </xsl:if>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          OC Total 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMOC_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          OC Mean 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMOC_MEAN_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          OC Median 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMOC_MED_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          OC Variance 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMOC_VAR2_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          OC Std Dev 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMOC_SD_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          AOH Total
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMAOH_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          AOH Mean 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMAOH_MEAN_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          AOH Median 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMAOH_MED_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          AOH Variance 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMAOH_VAR2_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          AOH Std Dev 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMAOH_SD_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          CAP Total 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMCAP_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          CAP Mean 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMCAP_MEAN_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          CAP Median 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMCAP_MED_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          CAP Variance 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMCAP_VAR2_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          CAP Std Dev 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMCAP_SD_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Total Total 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMTOTAL_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Total Mean 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMTOTAL_MEAN_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Total Median 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMTOTAL_MED_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Total Variance 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMTOTAL_VAR2_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Total Std Dev 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMTOTAL_SD_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Incent Total 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMINCENT_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Incent Mean 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMINCENT_MEAN_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Incent Median 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMINCENT_MED_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Incent Variance 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMINCENT_VAR2_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Incent Std Dev 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMINCENT_SD_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <xsl:if test="($localName != 'budgetoperation')">
        <h4 class="ui-bar-b">
          Net Total 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMNET_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Net Mean 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMNET_MEAN_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Net Median 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMNET_MED_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Net Variance 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMNET_VAR2_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        <h4 class="ui-bar-b">
          Net Std Dev 
        </h4>
			  <xsl:value-of select="DisplayComps:initValues($fullcolcount)"/>
			  <xsl:for-each select="@*">
				  <xsl:variable name="att_name" select="name()"/>
				  <xsl:variable name="att_value" select="."/>
				  <xsl:value-of select="DisplayComps:printValue('TAMNET_SD_', $att_name, $att_value)"/>
			  </xsl:for-each>
        <div class="ui-grid-a">
			    <xsl:value-of select="DisplayComps:DoPrintMobileValues($fullcolcount)"/>
        </div>
        </xsl:if>
      </div>
     </xsl:if>
	</xsl:template>
</xsl:stylesheet>
