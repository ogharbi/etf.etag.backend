
function LoadDependency() {
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12;
    try {
      
        write-host "info: SharePoint client is loaded" -foregroundcolor green
    }
    catch {
        write-host "info: SharePoint client is already loaded" -foregroundcolor red
    }
}
function InitColumn($name, $displayName, $displayNameFr, $fieldType, $required, $extra, $lookupList, $showField) {
    $id = [guid]::NewGuid().ToString()
    $c = New-Object -TypeName psobject 
    $c | Add-Member -MemberType NoteProperty -Name Name -Value $name
    $c | Add-Member -MemberType NoteProperty -Name DisplayName -Value $displayName
    $c | Add-Member -MemberType NoteProperty -Name DisplayNameFr -Value $displayNameFr
    $c | Add-Member -MemberType NoteProperty -Name FieldType -Value $fieldType
    $c | Add-Member -MemberType NoteProperty -Name Required -Value $required
    $c | Add-Member -MemberType NoteProperty -Name ID -Value $id
    $c | Add-Member -MemberType NoteProperty -Name Group -Value "VC"
    $c | Add-Member -MemberType NoteProperty -Name Extra -Value $extra
    $c | Add-Member -MemberType NoteProperty -Name LookupList -Value $lookupList
    $c | Add-Member -MemberType NoteProperty -Name ShowField -Value $showField
    return $c;
}
function AddFieldIndex($listId, $ColumnName) {
 
    #Get the Field from List
    $Field = Get-PnPField -List $listId -Identity $ColumnName
    if ($Field.Indexed -ne $true) {
        #Set the Indexed Property of the Field
        $Field.Indexed = $True
        $Field.Update() 
        $Context.ExecuteQuery() 
        Write-Host "$ColumnName indexed"
    }
}
function FieldXml($column, $context) {
    
   
    #create XML entry for a new field 
    $fieldAsXML = "<Field Type=`"$($column.FieldType)`" 
        DisplayName=`"$($column.DisplayName)`" 
        Name=`"$($column.Name)`" 
        StaticName=`"$($column.Name)`" 
        ID=`"$($column.ID)`" 
         Group=`"$($column.Group)`"
        Required=`"$($column.Required)`"  $($column.Extra) />"
    if (($column.FieldType -eq 'LookupMulti') -OR ($column.FieldType -eq 'Lookup')) {
        $web = $context.Web
        $context.Load($web)
        $context.ExecuteQuery()
        $LookupListID = $column.LookupList
        $LookupWebID = $web.Id
        $LookupField = $column.ShowField
        $fieldAsXML = "<Field Type=`"$($column.FieldType)`" 
            List=`"$($LookupListID)`"
            WebId=`"$($LookupWebID)`"
            ShowField=`"$($LookupField)`" 
            DisplayName=`"$($column.DisplayName)`" 
            Name=`"$($column.Name)`" 
            StaticName=`"$($column.Name)`" 
            ID=`"$($column.ID)`" 
             Group=`"$($column.Group)`"
            Required=`"$($column.Required)`"  $($column.Extra)  />"
            
    }
    elseif ($column.FieldType -eq 'Choice') {

        $ChoiceOptions = ""
        $Choices = $column.ChoiceValues.Split(",")
        foreach ($Choice in $Choices) {
            $ChoiceOptions += "<CHOICE>$Choice</CHOICE>"
        }
        #Define XML for Field Schema
        $fieldAsXML = "<Field Type='Choice' 
            ID=`"$($column.ID)`" 
             Group=`"$($column.Group)`"
            DisplayName=`"$($column.DisplayName)`" 
            Name=`"$($column.Name)`" 
            StaticName=`"$($column.Name)`" 
            Required=`"$($column.Required)`" 
            FillInChoice=`"FALSE`" 
            Format=`"$($column.Format)`"  $($column.Extra) >
            <Default>$($column.DefaultValue)</Default> <CHOICES>$ChoiceOptions</CHOICES></Field>"
    }
        
    elseif ($column.FieldType -eq 'TaxonomyFieldType') {
        $fieldAsXML = "<Field Type='TaxonomyFieldType' 
            ID=`"$($column.ID)`" 
             Group=`"$($column.Group)`"
            DisplayName=`"$($column.DisplayName)`" 
            Name=`"$($column.Name)`" 
            Required=`"$($column.Required)`"
            EnforceUniqueValues=`"FALSE`"  $($column.Extra) 
            />"
    }
    elseif ($column.FieldType -eq 'Calculated') {
        #Frame FieldRef Field
        $Formula = $column.Formula
        $FieldRefXML = ""
        $FieldRefs = $column.FieldsReferenced.Split(",")
        foreach ($Ref in $FieldRefs) {
            $FieldRefXML = $FieldRefXML + "<FieldRef Name='$Ref' />"
        }

        $fieldAsXML = "<Field Type='Calculated' 
            ID=`"$($column.ID)`" 
             Group=`"$($column.Group)`"
            DisplayName=`"$($column.DisplayName)`" 
            Name=`"$($column.Name)`" 
            Required=`"$($column.Required)`"
            ResultType=`"$($column.ResultType)`"  $($column.Extra) >
            <Formula>$Formula</Formula><FieldRefs>$FieldRefXML</FieldRefs></Field>"
    }
    return $fieldAsXML;

}
function FieldXml($column, $context) {
    
   
    #create XML entry for a new field 
    $fieldAsXML = "<Field Type=`"$($column.FieldType)`" 
        DisplayName=`"$($column.DisplayName)`" 
        Name=`"$($column.Name)`" 
        StaticName=`"$($column.Name)`" 
        ID=`"$($column.ID)`" 
         Group=`"$($column.Group)`"
        Required=`"$($column.Required)`"  $($column.Extra) />"
    if (($column.FieldType -eq 'LookupMulti') -OR ($column.FieldType -eq 'Lookup')) {
        $web = $context.Web
        $context.Load($web)
        $context.ExecuteQuery()
        $LookupListID = $column.LookupList
        $LookupWebID = $web.Id
        $LookupField = $column.ShowField
        $fieldAsXML = "<Field Type=`"$($column.FieldType)`" 
            List=`"$($LookupListID)`"
            WebId=`"$($LookupWebID)`"
            ShowField=`"$($LookupField)`" 
            DisplayName=`"$($column.DisplayName)`" 
            Name=`"$($column.Name)`" 
            StaticName=`"$($column.Name)`" 
            ID=`"$($column.ID)`" 
             Group=`"$($column.Group)`"
            Required=`"$($column.Required)`"  $($column.Extra)  />"
            
    }
    elseif ($column.FieldType -eq 'Choice') {

        $ChoiceOptions = ""
        $Choices = $column.ChoiceValues.Split(",")
        foreach ($Choice in $Choices) {
            $ChoiceOptions += "<CHOICE>$Choice</CHOICE>"
        }
        #Define XML for Field Schema
        $fieldAsXML = "<Field Type='Choice' 
            ID=`"$($column.ID)`" 
             Group=`"$($column.Group)`"
            DisplayName=`"$($column.DisplayName)`" 
            Name=`"$($column.Name)`" 
            StaticName=`"$($column.Name)`" 
            Required=`"$($column.Required)`" 
            FillInChoice=`"FALSE`" 
            Format=`"$($column.Format)`"  $($column.Extra) >
            <Default>$($column.DefaultValue)</Default> <CHOICES>$ChoiceOptions</CHOICES></Field>"
    }
        
    elseif ($column.FieldType -eq 'TaxonomyFieldType') {
        $fieldAsXML = "<Field Type='TaxonomyFieldType' 
            ID=`"$($column.ID)`" 
             Group=`"$($column.Group)`"
            DisplayName=`"$($column.DisplayName)`" 
            Name=`"$($column.Name)`" 
            Required=`"$($column.Required)`"
            EnforceUniqueValues=`"FALSE`"  $($column.Extra) 
            />"
    }
    elseif ($column.FieldType -eq 'Calculated') {
        #Frame FieldRef Field
        $Formula = $column.Formula
        $FieldRefXML = ""
        $FieldRefs = $column.FieldsReferenced.Split(",")
        foreach ($Ref in $FieldRefs) {
            $FieldRefXML = $FieldRefXML + "<FieldRef Name='$Ref' />"
        }

        $fieldAsXML = "<Field Type='Calculated' 
            ID=`"$($column.ID)`" 
             Group=`"$($column.Group)`"
            DisplayName=`"$($column.DisplayName)`" 
            Name=`"$($column.Name)`" 
            Required=`"$($column.Required)`"
            ResultType=`"$($column.ResultType)`"  $($column.Extra) >
            <Formula>$Formula</Formula><FieldRefs>$FieldRefXML</FieldRefs></Field>"
    }
    return $fieldAsXML;

}
function CreateListV2($web, $list, $url, $desc, $template) {
    $result = $null;
    $url = "$url"
    $existingList = GetListByUrl $web "$url"
    if ($existingList -eq $null) {
        New-PnPList -Title $list -Url $url -EnableContentTypes -Template $template
        $result = GetListByUrl $web "$url"
        Write-Host "List $($result.Title) created" -foregroundcolor green
    }
    else {
        $result = $existingList
        Write-Host "List $($existingList.Title) already exist" -foregroundcolor yellow
    }
    return $result
}
function AddCTToListV2($list, $ctName) {
    if ($null -ne $list) {
        Add-PnPContentTypeToList -List $list -ContentType $ctName -DefaultContentType
        Write-Host "Content type '$($ctName)' added to list '$($list)'"  -foregroundcolor green
        RemoveElementCtFromList $list
    }
}
function UpdateListView {
    param(
        [string]$list,
        $Fields,
        [Boolean]$addTitle = $true
    )
    $view = Get-PnPView -List $list
    if ($addTitle -eq $true) {
        $fields = @('Title') + $fields
    }
    $v = Set-PnPView -List $list -Identity $view[0].Title -Fields $fields
}
function GetListByUrl($web, $url) {
    Write-Host "List url : $url"
    $exist = $false
    try {
        $folder = $web.GetFolderByServerRelativeUrl("$($web.ServerRelativeUrl)/$($url)")
        $context.load($folder.Properties)
        $context.ExecuteQuery()  
        $ListId = [System.guid]::New($folder.Properties["vti_listname"].ToString())
        $List = $Web.Lists.GetById($ListId)
        $context.load($List)
        $context.ExecuteQuery()  
        $exist = $true
        return $List
    }
    catch {
        Write-Host "List $($url) NOT FOUND"
    } 
    if ($exist -eq $false) {
        return $null
    }
    else {
        return $List
    }
}
function AddContentType($ct, $parentCt) {
    $ctCheck = Get-PnPContentType $ct -erroraction 'silentlycontinue'
    if ($ctCheck -eq $null) {
        if ($parentCt -eq $null) {
            $newCt = Add-PnPContentType -Name $ct -Group "VC" 
        }
        else {
            $ctp = Get-PnPContentType $parentCt -InSiteHierarchy
            $newCt = Add-PnPContentType -Name $ct -Group "VC" -ParentContentType $ctp
        }
        Write-Host "Content type $($ct) created" -foregroundcolor green
    }
    else {
        Write-Host "Content type $($ct) already exist" -foregroundcolor yellow
    }
}
function AddFieldToCT($fname, $ct) {
    $f = Get-PnPField $fname -InSiteHierarchy
    if ($null -eq $f) {
        $f = Get-PnPField $fname
    }
    Add-PnPFieldToContentType -Field $f -ContentType $ct 
    Write-Host "Field $($fname) added to content type $($ct)"  -foregroundcolor green
}

function RemoveElementCtFromList($list) {
    $ct = Get-PnPContentType -List $list | Where { $_.Name -eq "Élément" -or $_.Name -eq "Document" -or $_.Name -eq "Item" }
    if ($null -ne $ct) {
        Remove-PnPContentTypeFromList -List $list -ContentType $ct
        Write-Host "Content type '$($ct.Name)' removed to list '$($list)'"  -foregroundcolor green
    }
}
function FieldsDefinitions() {
    $data = @(
        [pscustomobject]@{ key = "Col_Bu"; value = "<Field DisplayName='Agence' Type='Text' Required='FALSE' ID='edcd16f9-2ed9-44e5-973f-6e11f941727b'  StaticName='Col_Bu' Name='Col_Bu'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_AgFirstName"; value = "<Field DisplayName='Aiguilleur - Prénom' Type='Text' Required='FALSE' ID='fb3b2040-d106-4d86-b197-6737c30b3edb'  StaticName='Col_AgFirstName' Name='Col_AgFirstName'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_AgLastName"; value = "<Field DisplayName='Aiguilleur - Nom' Type='Text' Required='FALSE' ID='5502f7c1-1595-4bc2-83a1-3697e94fba1c'  StaticName='Col_AgLastName' Name='Col_AgLastName'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_FlFirstName"; value = "<Field DisplayName='Filleul - Prénom' Type='Text' Required='FALSE' ID='4afeddaa-e780-48e5-b633-311ff18e0969'  StaticName='Col_FlFirstName' Name='Col_FlFirstName'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_FlLastName"; value = "<Field DisplayName='Filleul - Nom' Type='Text' Required='FALSE' ID='da9a4228-c061-45a0-a002-6504365383f7'  StaticName='Col_FlLastName' Name='Col_FlLastName'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_AgUser"; value = "<Field Type='User' DisplayName='Aiguilleur' Name='Col_AgUser'  StaticName='Col_AgUser' ID='2440566b-19f8-4712-966e-1377f18babf5' Group='VC' Required='false'   />" }
        [pscustomobject]@{ key = "Col_StartDateT"; value = "<Field DisplayName='Date de début du tutorat' Type='DateTime' Required='FALSE' ID='260b4735-5a14-455d-afb7-847e1b40e251'  StaticName='Col_StartDateT' Name='Col_StartDateT'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_DurationM"; value = "<Field DisplayName='Durée envisagée (mois)' Type='Number' Required='FALSE' ID='49b54ab6-543e-44ca-9e84-de3b650dd0d5'  StaticName='Col_DurationM' Name='Col_DurationM'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Status"; value = "<Field DisplayName='Statut' Type='Text' Required='FALSE' ID='8c8d2cfa-c3a1-4e07-ac13-220a20e1f5c4'  StaticName='Col_Status' Name='Col_Status'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_StartDate"; value = "<Field DisplayName='Date de début' Type='DateTime' Required='FALSE' ID='64b112b6-a958-4e3b-9eaf-c18d7a7e89b2'  StaticName='Col_StartDate' Name='Col_StartDate'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_DueDate"; value = "<Field DisplayName='Date prévue' Type='DateTime' Required='FALSE' ID='62bf7a6a-b35e-4cdb-a8ee-c70b19210eeb'  StaticName='Col_DueDate' Name='Col_DueDate'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Participants"; value = "<Field DisplayName='Participants' Type='Text' Required='FALSE' ID='931378eb-521b-418c-bd77-0043285614d6'  StaticName='Col_Participants' Name='Col_Participants'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Ecoute"; value = "<Field DisplayName='Ecoute' Type='Number' Required='FALSE' ID='bb8ae7ee-043a-4b16-87db-bae26582f629'  StaticName='Col_Ecoute' Name='Col_Ecoute'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_QualityExchange"; value = "<Field DisplayName='Qualité échange' Type='Number' Required='FALSE' ID='197e74b2-9182-48e1-808e-7eca6c01f856'  StaticName='Col_QualityExchange' Name='Col_QualityExchange'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Autonomy"; value = "<Field DisplayName='Col_Autonomie' Type='Number' Required='FALSE' ID='6dd0228e-2ffe-4268-97ca-dae8466fc0ff'  StaticName='Col_Autonomy' Name='Col_Autonomy'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_EvAigComment"; value = "<Field DisplayName='Eval - Aiguilleur - Commentaire' Type='Note' Required='FALSE' ID='07fd69df-84f6-442d-adeb-57e507bcd520' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_EvAigComment' Name='Col_EvAigComment'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_EvFlComment"; value = "<Field DisplayName='Eval - Filleul - Commentaire' Type='Note' Required='FALSE' ID='25b702e4-ac81-47af-99b5-11f9870eab62' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_EvFlComment' Name='Col_EvFlComment'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Success"; value = "<Field DisplayName='Les succès' Type='Note' Required='FALSE' ID='9e57ee63-ffed-46fb-a5d9-fdec6b4e4252' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_Success' Name='Col_Success'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_SoftSkills"; value = "<Field DisplayName='Savoir-être' Type='Note' Required='FALSE' ID='6137ec71-0cfd-4edc-bee4-2f3cdf369ef2' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_SoftSkills' Name='Col_SoftSkills'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_MomMissing"; value = "<Field DisplayName='Moments manquants' Type='Note' Required='FALSE' ID='a369a42f-4b53-45c3-8f9a-75dd24b27aa1' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_MomMissing' Name='Col_MomMissing'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CompSec"; value = "<Field DisplayName='Comportement Sec/Env/Qua' Type='Note' Required='FALSE' ID='2fbc7637-72f1-4a13-ac18-b9c8ac9ef138' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CompSec' Name='Col_CompSec'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_EndDate"; value = "<Field DisplayName='Date de fin' Type='DateTime' Required='FALSE' ID='932d90ef-8c9d-4a05-8f19-762ea276cc65'  StaticName='Col_EndDate' Name='Col_EndDate'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_BlAigComment"; value = "<Field DisplayName='Bilan - Aiguilleur - Commentaire' Type='Note' Required='FALSE' ID='ed45af1e-2eaf-4a0a-81e2-e5abf6a74a48' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_BlAigComment' Name='Col_BlAigComment'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_BlFlComment"; value = "<Field DisplayName='Bilan - Filleul - Commentaire' Type='Note' Required='FALSE' ID='c75706db-2244-41d4-8953-ec7ab5512911' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_BlFlComment' Name='Col_BlFlComment'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Lesson"; value = "<Field DisplayName='Les leçons' Type='Note' Required='FALSE' ID='09e9fd81-3125-4b21-b0ce-aa77795b526f' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_Lesson' Name='Col_Lesson'  Group='VC' />" }
        #Related interview
        [pscustomobject]@{ key = "Col_Action1"; value = "<Field DisplayName='Action 1' Type='Note' Required='FALSE' ID='9087b0cb-0579-490a-b72a-81ce7a2e5a63' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_Action1' Name='Col_Action1'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Action2"; value = "<Field DisplayName='Action 2' Type='Note' Required='FALSE' ID='4d6507ba-334d-4bb1-b33b-2948cd336eab' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_Action2' Name='Col_Action2'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Comment"; value = "<Field DisplayName='Commentaire' Type='Note' Required='FALSE' ID='29c80bcd-806b-4878-befb-50bdc5ada814' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_Comment' Name='Col_Comment'  Group='VC' />" }
       

        #Mail Template
        [pscustomobject]@{ key = "Col_Subject"; value = "<Field DisplayName='Subject' Type='Text' Required='FALSE' ID='9f87b288-0656-40e6-a567-a9cfda92b27e'  StaticName='Col_Subject' Name='Col_Subject'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_BodyR"; value = "<Field DisplayName='Body' Type='Note' Required='FALSE' ID='4cca0220-222c-45bb-b3d3-7fadeeb5c8f6' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_BodyR' Name='Col_BodyR'  Group='VC' RichTextMode='FullHtml' RichText='TRUE' />" }
       
        # Links
        [pscustomobject]@{ key = "Col_LinkTarget"; value = "<Field Type='Choice' DisplayName='Target' ID='8d951170-61ee-4d3f-bb2c-5a604c373b94' Group='VC' Name='Col_LinkTarget' StaticName='Col_LinkTarget'  Required='false' FillInChoice='FALSE' ><Default>User</Default> <CHOICES><CHOICE></CHOICE><CHOICE>User</CHOICE><CHOICE>Admin</CHOICE></CHOICES></Field>" }
        [pscustomobject]@{ key = "Col_NewTab"; value = "<Field DisplayName='New tab' Type='Boolean' Required='FALSE' ID='544dc079-667b-477d-bd84-7d4689e2caa6'  StaticName='Col_NewTab' Name='Col_NewTab'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_LinkUrl"; value = "<Field DisplayName='Url' Type='URL' Format='Hyperlink'  Required='FALSE' ID='45e882b9-7b06-47aa-96dd-66ad04c1212b'  StaticName='Col_LinkUrl' Name='Col_LinkUrl'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Order"; value = "<Field DisplayName='Order' Type='Number' Required='FALSE' ID='4d8086a0-0018-4261-9efe-ed778cc65d1d'  StaticName='Col_Order' Name='Col_Order'  Group='VC' />" }
    
        [pscustomobject]@{ key = "Col_E_Code"; value = "<Field DisplayName='Code' Type='Text' Required='FALSE' ID='7ae8799c-ccbe-4178-af8a-847b89038bcd'  StaticName='Col_E_Code' Name='Col_E_Code'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Author"; value = "<Field Type='User' DisplayName='Créé par' Name='Col_Author'  StaticName='Col_Author' ID='f654905d-94ce-44bf-a705-79706fc0bd09' Group='VC' Required='false'  ShowInEditForm='FALSE' ShowInNewForm='FALSE' Indexed='TRUE'/>" }
        [pscustomobject]@{ key = "Col_Editor"; value = "<Field Type='User' DisplayName='Modifié par' Name='Col_Editor'  StaticName='Col_Editor' ID='b26c0826-6c0c-4948-9b72-219c1b822d3b' Group='VC' Required='false'  ShowInEditForm='FALSE'  ShowInNewForm='FALSE'  />" }
        [pscustomobject]@{ key = "Col_ParentId"; value = "<Field DisplayName='Parent Id' Type='Number' Required='FALSE' ID='d82d8d65-af2a-4451-ad88-b65fcc4b140b'  StaticName='Col_ParentId' Name='Col_ParentId'  Group='VC' Indexed='TRUE'/>" }
        [pscustomobject]@{ key = "Col_Guid"; value = "<Field DisplayName='Guid' Type='Text' Required='FALSE' ID='88dd37ce-8ae4-4f66-af6d-db16241a1487'  StaticName='Col_Guid' Name='Col_Guid'  Group='VC' Indexed='TRUE'/>" }
        [pscustomobject]@{ key = "Col_Order"; value = "<Field DisplayName='Order' Type='Number' Required='FALSE' ID='c2f421a2-7fb5-4bce-95ca-5b6fbfdf784e'  StaticName='Col_Order' Name='Col_Order'  Group='VC' Indexed='TRUE'/>" }
        
        
    )
    return $data;
}

function CreateFields() {
    $data = FieldsDefinitions
    Write-Host $data.Count
    $ctx = Get-PnPContext 
    $fields = $ctx.Web.AvailableFields
    $ctx.Load($fields)
    $ctx.ExecuteQuery()

    $data | ForEach-Object {
        $key = $_.key
        $value = $_.value
   
        $field = $fields |  where { $_.InternalName -eq $key }
        if ($field -eq $null ) {
            Add-PnPFieldFromXml -FieldXml $value
            write-host "Field $($key) created" -foregroundcolor green
        }
        else {
            write-host "Field $($key) already exist" -foregroundcolor yellow
        }
    }
}
function CreateLkFields($ctx,$web){
    $list = GetListByUrl $web "Lists/vc_interview" 
    $fieldsToCreate += InitColumn "Col_Lk_Request" "Fiche de suivi" "" "Lookup" "false" "   EnforceUniqueValues='FALSE'" $list.Id "Title"
    foreach ($f in $fieldsToCreate) {
        $fieldExist = Get-PnPField $f.Name -erroraction 'silentlycontinue'
        $key = $f.Name
        if ($null -eq $fieldExist) {
            $xml = FieldXml $f $ctx
            Add-PnPFieldFromXml -FieldXml $xml
            write-host "Field $($key) created" -foregroundcolor green
        }
        else {
            write-host "Field $($key) already exist" -foregroundcolor yellow
        }
    }
}
function LinkFields() {
    $data = @(
        "Col_LinkUrl"
        "Col_LinkTarget"
        "Col_NewTab"
        "Col_Order"
    )
    return $data;
}

function MailTemplateCT() {
    
    $ct = "VC Mail template"
    AddContentType $ct
    $fields = LinkFields
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
    return $fields;
} 
function LinkCT() {
    $ct = "VC Link"
    AddContentType $ct
    $fields = LinkFields 
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
} 
function AppSettingsFields() {
    "Col_E_Code"
}
function AppSettingsCT() {
    $ct = "VC Settings"
    AddContentType $ct
    $fields = AppSettingsFields 
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
}
function BuFields() {
    "Col_E_Code"
}
function BuCT() {
    $ct = "VC BU"
    AddContentType $ct
    $fields = BuFields 
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
}
function MailTemplateFields() {
    $fields = @(
        "Col_E_Code"
        "Col_Subject"
        "Col_BodyR"
    )
    return $fields;
}
function MailTemplateCT() {
    
    $ct = "VC Mail template"
    AddContentType $ct
    $fields = MailTemplateFields
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
    return $fields;
}
function AttachmentFields() {
    $fields = @(
        "Col_E_Code"
        "Col_ParentId"
        "Col_Author"
        "Col_Editor"
    )
    return $fields;
}
function AttachmentsCT($addParentFields) {
    $ct = "VC Documents"
    AddContentType $ct "Document"
    $fields = AttachmentFields
    if($addParentFields){
        $fields+=ParentFields
    }
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
}

function InterviewFields() {
    $data = @(
        "Col_Bu"
        "Col_AgUser"
        "Col_FlFirstName"
        "Col_FlLastName"
        "Col_StartDateT"
        "Col_DurationM"
        "Col_Status"
        "Col_StartDate"
        "Col_Participants"
        "Col_Ecoute"
        "Col_QualityExchange"
        "Col_Autonomy"
        "Col_EvAigComment"
        "Col_EvFlComment"
        "Col_EndDate"
        "Col_BlAigComment"
        "Col_BlFlComment"
        "Col_Lesson"
        "Col_Author"
        "Col_Editor"
    )
    return $data;
}
function interviewCT(){
      $ct = "VC Interview"
    AddContentType $ct
    $fields = InterviewFields
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
    return $fields;
}
function QuartlyInterviewFields() {
    $data = @(
        "Col_DueDate"    
        "Col_StartDate"
        "Col_Status"
        "Col_MomMissing"
        "Col_Success"
        "Col_SoftSkills"
        "Col_CompSec"
        "Col_Guid"
        "Col_Order"
    )
    return $data;
}
function QuartlyInterviewCT($addParentFields){
      $ct = "VC Quartly interview"
    AddContentType $ct
    $fields = QuartlyInterviewFields
    if($addParentFields){
        $fields+=ParentFields
    }
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
    return $fields;
}
function RelatedInterviewFields() {
    $fields = @(
        "Col_Action1"
        "Col_Action2"
        "Col_Status"
        "Col_Comment"
        "Col_Guid"
        "Col_Order"
        "Col_Author"
        "Col_Editor"
    )
    return $fields;
}
function RelatedInterviewCT($addParentFields){
      $ct = "VC Related interview"
    AddContentType $ct
    $fields = RelatedInterviewFields
    if($addParentFields){
        $fields+=ParentFields
    }
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
    return $fields;
}
function AttachmentsNewList($web, $addSiteFields) {

    $url = "vc_attachments"
    $list = "App - Documents"
    $ct = "VC Documents"
    $fields = AttachmentFields
    if ($addSiteFields) {
        $fields += ParentFields
    }
    CreateListV2 $web $list $url $listDesc "DocumentLibrary"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    UpdateListView -List $l.Id -Fields $fields
    AddFieldIndex  $l.Id "Col_ParentId"
    AddFieldIndex  $l.Id "Col_Lk_Request"
    AddFieldIndex  $l.Id "Created"
    AddFieldIndex  $l.Id "Modified"

}
function InterviewList($web) {
    $url = "Lists/vc_interview"
    $list = "App - Carnets de bord"
    $ct = "VC Interview"
    $fields = InterviewFields 
    CreateListV2 $web $list $url $listDesc "GenericList"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    UpdateListView -List $l.Id -Fields $fields
}
function ParentFields(){
    $fields = @(
      "Col_Lk_Request"
    )
    return $fields;
}
function RelatedInterviewList($web,$addSiteFields) {
    $url = "Lists/vc_relatedinterview"
    $list = "App - Actions"
    $ct = "VC Related interview"
    $fields = RelatedInterviewFields 
    if ($addSiteFields) {
            $fields += ParentFields
    }
        $l = GetListByUrl $web "$url"
        CreateListV2 $web $list $url $listDesc "GenericList"
        $l = GetListByUrl $web "$url"
        AddCTToListV2 $l.Id $ct
        UpdateListView -List $l.Id -Fields $fields
    AddFieldIndex  $l.Id "Col_Lk_Request"
    AddFieldIndex  $l.Id "Created"
    AddFieldIndex  $l.Id "Modified"
    AddFieldIndex  $l.Id "Title"
}
function QuartlyInterviewList($web,$addSiteFields) {
    $url = "Lists/vc_quartlyinterview"
    $list = "App - Entretiens trimestiels"
    $ct = "VC Quartly interview"
    $fields = QuartlyInterviewFields
    if ($addSiteFields) {
            $fields += ParentFields
    }
        $l = GetListByUrl $web "$url"
        CreateListV2 $web $list $url $listDesc "GenericList"
        $l = GetListByUrl $web "$url"
        AddCTToListV2 $l.Id $ct
        UpdateListView -List $l.Id -Fields $fields
    AddFieldIndex  $l.Id "Col_Lk_Request"
    AddFieldIndex  $l.Id "Created"
    AddFieldIndex  $l.Id "Modified"

}
function MailTemplateList($web) {
    $url = "Lists/vc_mailtemplate"
    $list = "App - Mail templates"
    $ct = "VC Mail template"
    $fields = MailTemplateFields 
    CreateListV2 $web $list $url $listDesc "GenericList"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    UpdateListView -List $l.Id -Fields $fields
}
function AppSettingsNewList($web) {
    $url = "Lists/vc_settings"
    $list = "App - Paramètres"
    $ct = "VC Settings"
    CreateListV2 $web $list $url $listDesc "GenericList"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    $fields = AppSettingsFields
    UpdateListView -List $l.Id -Fields $fields
}
function BuNewList($web) {
    $url = "Lists/vc_Bu"
    $list = "App - Agences"
    $ct = "VC BU"
    CreateListV2 $web $list $url $listDesc "GenericList"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    $fields = AppSettingsFields
    UpdateListView -List $l.Id -Fields $fields
}
function LinkNewList($web) {
    $url = "Lists/vc_link"
    $list = "App - Links"
    $ct = "VC Link"
    $fields = LinkFields 
    CreateListV2 $web $list $url $listDesc "GenericList"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    UpdateListView -List $l.Id -Fields $fields
}
function SetStructureRoot($siteURL) {
    
    $cult = Get-Culture
    write-host "Connection to $siteURL : $($cult.Name)" -foregroundcolor green
    
    Connect-PnPOnline -Url $siteURL  -Interactive -ClientId "810c1e36-dab8-4f95-8cdc-ad766fdcfd7e"
    $context = Get-PnPContext
    write-host "Context : $($context.Url)"
    $web = $context.Web
    $context.Load($web)
    $context.ExecuteQuery()

    CreateFields
    InterviewCT
    RelatedInterviewCT $false
    QuartlyInterviewCT $false
    AttachmentsCT $false
    LinkCT
    MailTemplateCT
    AppSettingsCT
    BuCT
 
    # InterviewList $web
    # CreateLkFields  $context $web
    # MailTemplateList $web
    # LinkNewList $web
    # AppSettingsNewList $web
    # BuNewList $web
    # RelatedInterviewCT $true
    # RelatedInterviewList $web
    # QuartlyInterviewCT $true
    # QuartlyInterviewList $web
    # AttachmentsCT $true
    # AttachmentsNewList $web $true

}
$siteURL = "https://vincic.sharepoint.com/sites/etag-dev"
#$siteURL = "https://futur365.sharepoint.com/sites/Dev08"

cls
LoadDependency
$newInstall = $true
SetStructureRoot $siteURL