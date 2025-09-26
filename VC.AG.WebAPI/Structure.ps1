
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
        #$Fields = @('Title') + $fields
    }
    $v = Set-PnPView -List $list -Identity $view[0].Title -Fields $Fields
}
function AddListView2 {
    param(
        $ctx,
        [String]$List,
        [String] $Title,
        $Fields,
        $Query
    )
   
    Write-Host "Looking for view $Title"
    $view = Get-PnPView -List $List -Identity $Title -erroraction 'silentlycontinue'
    if ($view -eq $null) {
        Add-PnPView -List $List -Title $Title -Fields $Fields -Query $Query
        Write-host "View $Title added to list $list" -foregroundcolor green
    }
    else {
        if ($null -eq $query) {
            Set-PnPView -List $List -Identity $view.Title -Fields $Fields
        }
        else {
            $targetList = $Ctx.Web.Lists.GetById($List)
 
            #Get the view to update
            $targetView = $targetList.Views.GetByTitle($view.Title)
            $ctx.ExecuteQuery()
            $targetView.ViewQuery = $Query
            $targetView.Update()
            $ctx.ExecuteQuery()
            Set-PnPView -List $List -Identity $view.Title -Fields $Fields
        }
        Write-host "View $Title updated for list $list" -foregroundcolor green
    }
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
        #Commun
        [pscustomobject]@{ key = "Col_FullName"; value = "<Field DisplayName='Nom / prénom' Type='Text' Required='FALSE' ID='C7EC4B3D-1AFB-41CF-8933-23472481316A'  StaticName='Col_FullName' Name='Col_FullName'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Tel"; value = "<Field DisplayName='Téléphone' Type='Text' Required='FALSE' ID='A24C1AFD-9922-4396-9BF2-541A4FADD62C'  StaticName='Col_Tel' Name='Col_Tel'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Email"; value = "<Field DisplayName='Email' Type='Text' Required='FALSE' ID='92E47F3C-5E3D-4165-8A1F-797A21108BB3'  StaticName='Col_Email' Name='Col_Email'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Position"; value = "<Field DisplayName='Poste' Type='Text' Required='FALSE' ID='3AF470B3-BA4B-4698-8228-5B4006DDF8EB'  StaticName='Col_Position' Name='Col_Position'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Promotion"; value = "<Field DisplayName='Promotion' Type='Text' Required='FALSE' ID='0E6E46DC-9486-4E77-A329-9F62941CBB27'  StaticName='Col_Promotion' Name='Col_Promotion'  Group='VC' />" }
        
        [pscustomobject]@{ key = "Col_StartDate"; value = "<Field DisplayName='Date de début' Type='DateTime' Required='FALSE' ID='64b112b6-a958-4e3b-9eaf-c18d7a7e89b2'  StaticName='Col_StartDate' Name='Col_StartDate'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_EndDate"; value = "<Field DisplayName='Date de fin' Type='DateTime' Required='FALSE' ID='932d90ef-8c9d-4a05-8f19-762ea276cc65'  StaticName='Col_EndDate' Name='Col_EndDate'  Group='VC' />" }
        
        [pscustomobject]@{ key = "Col_RespUser"; value = "<Field Type='User' DisplayName='Responsable' Name='Col_RespUser'  StaticName='Col_RespUser' ID='E4C06628-0860-4962-B138-8FF8109A5541' Group='VC' Required='false'   />" }
        [pscustomobject]@{ key = "Col_RespFullName"; value = "<Field DisplayName='Responsable : nom complet' Type='Text' Required='FALSE' ID='3C3E4066-9D24-47AC-9982-BF021AB7A81E'  StaticName='Col_RespFullName' Name='Col_RespFullName'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_RespUserSPId"; value = "<Field Type='Number' DisplayName='Responsable ID' Name='Col_RespUserSPId'  StaticName='Col_RespUserSPId' ID='94F9245E-0977-4507-ADB7-F267E902F08B' Group='VC' Required='false'   />" }
        [pscustomobject]@{ key = "Col_RespPosition"; value = "<Field DisplayName='Responsable : fonction' Type='Text' Required='FALSE' ID='FC7190E1-B0A0-4916-8B81-879B27EC92EF'  StaticName='Col_RespPosition' Name='Col_RespPosition'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Bu"; value = "<Field DisplayName='Agence' Type='Text' Required='FALSE' ID='edcd16f9-2ed9-44e5-973f-6e11f941727b'  StaticName='Col_Bu' Name='Col_Bu'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_DirGeneral"; value = "<Field DisplayName='Direction générale' Type='Text' Required='FALSE' ID='5BD32A58-F985-43FE-B4D7-4B92B62192FE'  StaticName='Col_DirGeneral' Name='Col_DirGeneral'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_UrlR"; value = "<Field DisplayName='URL' Type='Note' Required='FALSE' ID='605145bc-de13-4e04-9c53-2735332160c6' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_UrlR' Name='Col_UrlR'  Group='VC' RichTextMode='FullHtml' RichText='TRUE' />" }
        [pscustomobject]@{ key = "Col_FormType"; value = "<Field DisplayName='Type de formulaire' Type='Text' Required='FALSE' ID='2FBC8417-20C6-4AF1-9B51-93B6EA483288'  StaticName='Col_FormType' Name='Col_FormType'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_FormTarget"; value = "<Field DisplayName='Périmètre' Type='Text' Required='FALSE' ID='1FFDF589-9340-4B44-BEA0-154B8D41E0E4'  StaticName='Col_FormTarget' Name='Col_FormTarget'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Status"; value = "<Field DisplayName='Statut' Type='Text' Required='FALSE' ID='8c8d2cfa-c3a1-4e07-ac13-220a20e1f5c4'  StaticName='Col_Status' Name='Col_Status'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Guid"; value = "<Field DisplayName='Guid' Type='Text' Required='FALSE' ID='88dd37ce-8ae4-4f66-af6d-db16241a1487'  StaticName='Col_Guid' Name='Col_Guid'  Group='VC' Indexed='TRUE'/>" }
        [pscustomobject]@{ key = "Col_Author"; value = "<Field Type='User' DisplayName='Créé par' Name='Col_Author'  StaticName='Col_Author' ID='f654905d-94ce-44bf-a705-79706fc0bd09' Group='VC' Required='false'  ShowInEditForm='FALSE' ShowInNewForm='FALSE' Indexed='TRUE'/>" }
        [pscustomobject]@{ key = "Col_Editor"; value = "<Field Type='User' DisplayName='Modifié par' Name='Col_Editor'  StaticName='Col_Editor' ID='b26c0826-6c0c-4948-9b72-219c1b822d3b' Group='VC' Required='false'  ShowInEditForm='FALSE'  ShowInNewForm='FALSE'  />" }
        
        [pscustomobject]@{ key = "Col_RespSignture"; value = "<Field DisplayName='Responsable - signature' Type='Note' Required='FALSE' ID='85fda0d1-6f36-41d2-ad33-5581e6192e5f' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_RespSignture' Name='Col_RespSignture'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_RespSignName"; value = "<Field DisplayName='Responsable - Signature - Nom' Type='Text' Required='FALSE' ID='1f4c5034-1d4d-4f81-8158-220411a08d2c'  StaticName='Col_RespSignName' Name='Col_RespSignName'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_RespSignDate"; value = "<Field DisplayName='Responsable - Signature - Date' Type='DateTime' Required='FALSE' ID='09ae074e-c3e5-4e06-b571-69fd89a4c20c'  StaticName='Col_RespSignDate' Name='Col_RespSignDate'  Group='VC' />" }

        [pscustomobject]@{ key = "Col_CandSignture"; value = "<Field DisplayName='Candidat - signature' Type='Note' Required='FALSE' ID='16126A88-B23E-4E55-AE0B-E288EC5CEE19' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CandSignture' Name='Col_CandSignture'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CandSignName"; value = "<Field DisplayName='Candidat - Signature - Nom' Type='Text' Required='FALSE' ID='51A7E597-7016-4A28-A78A-6251FC2ED5C8'  StaticName='Col_CandSignName' Name='Col_CandSignName'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CandSignDate"; value = "<Field DisplayName='Candidat - Signature - Date' Type='DateTime' Required='FALSE' ID='CC84997E-5522-4146-8484-A41120E03AD3'  StaticName='Col_CandSignDate' Name='Col_CandSignDate'  Group='VC' />" }

        [pscustomobject]@{ key = "Col_CtrRootReq"; value = "<Field DisplayName='Root request' Type='Boolean' Required='FALSE' ID='B48906E9-3B3D-4670-A0F6-24BC446264E4'  StaticName='Col_CtrRootReq' Name='Col_CtrRootReq'  Group='VC' />" }

        #Aiguilleur
    
        [pscustomobject]@{ key = "Col_RespUser2"; value = "<Field Type='User' DisplayName='Responsable2' Name='Col_RespUser2'  StaticName='Col_RespUser2' ID='c974172e-fccb-46c9-8e09-0104d90c2899' Group='VC' Required='false'   />" }
        [pscustomobject]@{ key = "Col_RespFullName2"; value = "<Field DisplayName='Responsable2_Nom complet' Type='Text' Required='FALSE' ID='a371245c-833a-422a-88ce-9c1f7f783505'  StaticName='Col_RespFullName2' Name='Col_RespFullName2'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_RespUserSPId2"; value = "<Field Type='Number' DisplayName='Responsable2_ID' Name='Col_RespUserSPId2'  StaticName='Col_RespUserSPId2' ID='6f6385ec-c8f3-429c-89e3-5db4d39a80ed' Group='VC' Required='false'   />" }
        [pscustomobject]@{ key = "Col_StartDateT"; value = "<Field DisplayName='Date de début du tutorat' Type='DateTime' Required='FALSE' ID='260b4735-5a14-455d-afb7-847e1b40e251'  StaticName='Col_StartDateT' Name='Col_StartDateT'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_DurationM"; value = "<Field DisplayName='Durée envisagée' Type='Number' Required='FALSE' ID='49b54ab6-543e-44ca-9e84-de3b650dd0d5'  StaticName='Col_DurationM' Name='Col_DurationM'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_DueDate"; value = "<Field DisplayName='Date prévue' Type='DateTime' Required='FALSE' ID='62bf7a6a-b35e-4cdb-a8ee-c70b19210eeb'  StaticName='Col_DueDate' Name='Col_DueDate'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Participants"; value = "<Field DisplayName='Participants' Type='Text' Required='FALSE' ID='931378eb-521b-418c-bd77-0043285614d6'  StaticName='Col_Participants' Name='Col_Participants'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Ecoute"; value = "<Field DisplayName='Ecoute' Type='Number' Required='FALSE' ID='bb8ae7ee-043a-4b16-87db-bae26582f629'  StaticName='Col_Ecoute' Name='Col_Ecoute'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_QualityExchange"; value = "<Field DisplayName='Qualité échange' Type='Number' Required='FALSE' ID='197e74b2-9182-48e1-808e-7eca6c01f856'  StaticName='Col_QualityExchange' Name='Col_QualityExchange'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Autonomy"; value = "<Field DisplayName='Autonomie' Type='Number' Required='FALSE' ID='6dd0228e-2ffe-4268-97ca-dae8466fc0ff'  StaticName='Col_Autonomy' Name='Col_Autonomy'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_EvAigComment"; value = "<Field DisplayName='Eval - Aiguilleur - Commentaire' Type='Note' Required='FALSE' ID='07fd69df-84f6-442d-adeb-57e507bcd520' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_EvAigComment' Name='Col_EvAigComment'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_EvFlComment"; value = "<Field DisplayName='Eval - Filleul - Commentaire' Type='Note' Required='FALSE' ID='25b702e4-ac81-47af-99b5-11f9870eab62' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_EvFlComment' Name='Col_EvFlComment'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Success"; value = "<Field DisplayName='Les succès' Type='Note' Required='FALSE' ID='9e57ee63-ffed-46fb-a5d9-fdec6b4e4252' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_Success' Name='Col_Success'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_SoftSkills"; value = "<Field DisplayName='Savoir-être' Type='Note' Required='FALSE' ID='6137ec71-0cfd-4edc-bee4-2f3cdf369ef2' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_SoftSkills' Name='Col_SoftSkills'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_MomMissing"; value = "<Field DisplayName='Moments manquants' Type='Note' Required='FALSE' ID='a369a42f-4b53-45c3-8f9a-75dd24b27aa1' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_MomMissing' Name='Col_MomMissing'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CompSec"; value = "<Field DisplayName='Comportement Sec/Env/Qua' Type='Note' Required='FALSE' ID='2fbc7637-72f1-4a13-ac18-b9c8ac9ef138' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CompSec' Name='Col_CompSec'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_BlAigComment"; value = "<Field DisplayName='Bilan - Aiguilleur - Commentaire' Type='Note' Required='FALSE' ID='ed45af1e-2eaf-4a0a-81e2-e5abf6a74a48' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_BlAigComment' Name='Col_BlAigComment'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_BlFlComment"; value = "<Field DisplayName='Bilan - Filleul - Commentaire' Type='Note' Required='FALSE' ID='c75706db-2244-41d4-8953-ec7ab5512911' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_BlFlComment' Name='Col_BlFlComment'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_Lesson"; value = "<Field DisplayName='Les leçons' Type='Note' Required='FALSE' ID='09e9fd81-3125-4b21-b0ce-aa77795b526f' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_Lesson' Name='Col_Lesson'  Group='VC' />" }
        
        #Contrat : alternant
        [pscustomobject]@{ key = "Col_CtrHiringDate"; value = "<Field DisplayName='Date embauche' Type='DateTime' Required='FALSE' ID='456EBE25-2D72-4FB5-8074-447BDCD6F5D4'  StaticName='Col_CtrHiringDate' Name='Col_CtrHiringDate'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrMeetDate"; value = "<Field DisplayName='Date bilan' Type='DateTime' Required='FALSE' ID='59C06D66-1E0D-473A-AA61-D22CF7F76715'  StaticName='Col_CtrMeetDate' Name='Col_CtrMeetDate'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrEcole"; value = "<Field DisplayName='Ecole' Type='Text' Required='FALSE' ID='D0CE0A07-A5D9-429F-A455-0E45B924CB70'  StaticName='Col_CtrEcole' Name='Col_CtrEcole'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrDiplome"; value = "<Field DisplayName='Diplôme' Type='Text' Required='FALSE' ID='94173C71-C5C3-4A23-86B4-C7443B2605A0'  StaticName='Col_CtrDiplome' Name='Col_CtrDiplome'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrDescMission"; value = "<Field DisplayName='Description des missions confiées' Type='Note' Required='FALSE' ID='39132B7C-1740-4CFB-BEDB-F867C1E245E5' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrDescMission' Name='Col_CtrDescMission'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrComp"; value = "<Field DisplayName='Compétences acquises' Type='Note' Required='FALSE' ID='78EEA0A1-5F22-4AE2-B0D3-90FF7DBCB53B' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrComp' Name='Col_CtrComp'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrTypeCh"; value = "<Field DisplayName='Type de chantiers confiés' Type='Note' Required='FALSE' ID='F5AF20C5-2133-4131-9536-B0B221475021' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrTypeCh' Name='Col_CtrTypeCh'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrPositionETF"; value = "<Field DisplayName='Poste chez ETF / VC' Type='Boolean' Required='FALSE' ID='5DD4CE58-480B-4F0D-9B46-B3E0F7E870FA'  StaticName='Col_CtrPositionETF' Name='Col_CtrPositionETF'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrPositionETFDesc"; value = "<Field DisplayName='Poste : description' Type='Text' Required='FALSE' ID='DF74C352-4425-4236-9FB2-1AF83A449067'  StaticName='Col_CtrPositionETFDesc' Name='Col_CtrPositionETFDesc'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrMobility"; value = "<Field DisplayName='Mobilité' Type='Text' Required='FALSE' ID='FDD92AA2-15B4-45B5-84A1-3BA07D423F18'  StaticName='Col_CtrMobility' Name='Col_CtrMobility'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrMobilityReg"; value = "<Field DisplayName='Mobilité - Région' Type='Text' Required='FALSE' ID='7D401293-BCC7-4B56-A066-FB700BF37EEA'  StaticName='Col_CtrMobilityReg' Name='Col_CtrMobilityReg'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrPojectPro"; value = "<Field DisplayName='Projet professionnel' Type='Text' Required='FALSE' ID='21FE1A2B-275B-46F7-A590-7B7E6C63EF00'  StaticName='Col_CtrPojectPro' Name='Col_CtrPojectPro'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrRecoETF"; value = "<Field DisplayName='ETF recommandé' Type='Boolean' Required='FALSE' ID='FDF53F0B-B914-4689-8B07-0730A55FEDD1'  StaticName='Col_CtrRecoETF' Name='Col_CtrRecoETF'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrRecoEtud"; value = "<Field DisplayName='Stagiaire recommandé' Type='Boolean' Required='FALSE' ID='AF4C1462-3A05-454E-9307-306164B70A7D'  StaticName='Col_CtrRecoEtud' Name='Col_CtrRecoEtud'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrEmbaucheETF"; value = "<Field DisplayName='Embauche en agence' Type='Boolean' Required='FALSE' ID='08AA08FB-2057-4B89-8D73-B487A05163B8'  StaticName='Col_CtrEmbaucheETF' Name='Col_CtrEmbaucheETF'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrObs"; value = "<Field DisplayName='Observations' Type='Note' Required='FALSE' ID='9DFFE5D2-DE8D-40C3-B226-670FA645EF71' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrObs' Name='Col_CtrObs'  Group='VC' />" }

        #Chef chantier
        [pscustomobject]@{ key = "Col_CtrIntAgence"; value = "<Field DisplayName='Intégration agence' Type='Note' Required='FALSE' ID='8F5FD199-F464-45AB-BBE1-D9694BCF470F' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrIntAgence' Name='Col_CtrIntAgence'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrIntPromo"; value = "<Field DisplayName='Intégration promotion' Type='Note' Required='FALSE' ID='78B2139D-3B92-4718-87D1-C54CB5F450D6' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrIntPromo' Name='Col_CtrIntPromo'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrPtPos"; value = "<Field DisplayName='Points positifs année' Type='Note' Required='FALSE' ID='31E01AD6-9D56-4239-944B-E5DC8EEF21D3' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrPtPos' Name='Col_CtrPtPos'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrlAccComment"; value = "<Field DisplayName='Accueillir - Commentaire' Type='Note' Required='FALSE' ID='453DDDBD-CE9D-498E-8ED5-EBCA3C4447FA' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrlAccComment' Name='Col_CtrlAccComment'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrSavFComment"; value = "<Field DisplayName='Savoir-faire - Commentaire' Type='Note' Required='FALSE' ID='8532FC35-3290-4BC7-9ADB-7DB807013DB1' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrSavFComment' Name='Col_CtrSavFComment'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrSupComment"; value = "<Field DisplayName='Superviser - Commentaire' Type='Note' Required='FALSE' ID='69C8BDAB-0A5E-4E10-A061-250C96E549CC' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrSupComment' Name='Col_CtrSupComment'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrProgComment"; value = "<Field DisplayName='Progression - Commentaire' Type='Note' Required='FALSE' ID='6FC80030-52A0-4993-9BCB-8A3DEFCE908C' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrProgComment' Name='Col_CtrProgComment'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrFormComment"; value = "<Field DisplayName='Formation - Commentaire' Type='Note' Required='FALSE' ID='9AECCDC6-E7E9-4003-8620-746A52CD3E86' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrFormComment' Name='Col_CtrFormComment'  Group='VC' />" }

        [pscustomobject]@{ key = "Col_CtrSecFer"; value = "<Field DisplayName='Séc. ferroviaire' Type='Boolean' Required='FALSE' ID='A39B3EE2-1994-4C0D-82F5-BE93BD1C4CC8'  StaticName='Col_CtrSecFer' Name='Col_CtrSecFer'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrSecFerDuree"; value = "<Field DisplayName='Séc. ferroviaire : durée' Type='Text' Required='FALSE' ID='D9843C90-799D-4639-AA35-74EE6A235861'  StaticName='Col_CtrSecFer' Name='Col_CtrSecFerDuree'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrSecFerAgence"; value = "<Field DisplayName='Séc. ferroviaire : agence' Type='Text' Required='FALSE' ID='9012B6DE-5030-496F-AD43-078949DA2FE3'  StaticName='Col_CtrSecFerAgence' Name='Col_CtrSecFerAgence'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrSecFerMission"; value = "<Field DisplayName='Séc. ferroviaire : missions' Type='Note' Required='FALSE' ID='A888DE28-509E-4020-BAF8-D64E289EAD73' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrSecFerMission' Name='Col_CtrSecFerMission'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrSecFerMissioné"; value = "<Field DisplayName='Séc. ferroviaire : missions 2' Type='Note' Required='FALSE' ID='AC88D819-FB92-4568-901D-24B691DCE01C' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrSecFerMissioné' Name='Col_CtrSecFerMissioné'  Group='VC' />" }
        
        [pscustomobject]@{ key = "Col_CtrPrev"; value = "<Field DisplayName='Prévention' Type='Boolean' Required='FALSE' ID='3E46BA9A-1558-405B-AF86-F47A9CF16F0D'  StaticName='Col_CtrPrev' Name='Col_CtrPrev'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrPrevDuree"; value = "<Field DisplayName='Prévention : durée' Type='Text' Required='FALSE' ID='D705CD33-7CCC-4BD9-AE8C-F03E022247CB'  StaticName='Col_CtrPrev' Name='Col_CtrPrevDuree'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrPrevAgence"; value = "<Field DisplayName='Prévention : agence' Type='Text' Required='FALSE' ID='7598A444-2AD7-4BA9-BB1C-FB07BDF77A33'  StaticName='Col_CtrPrevAgence' Name='Col_CtrPrevAgence'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrPrevMission"; value = "<Field DisplayName='Prévention : missions' Type='Note' Required='FALSE' ID='015307E9-B9C5-4A72-B988-84F0A23721B7' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrPrevMission' Name='Col_CtrPrevMission'  Group='VC' />" }
        
        [pscustomobject]@{ key = "Col_CtrMat"; value = "<Field DisplayName='Matériel' Type='Boolean' Required='FALSE' ID='B9352561-E2A9-4480-81BE-DBEF28075BB1'  StaticName='Col_CtrMat' Name='Col_CtrMat'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrMatDuree"; value = "<Field DisplayName='Matériel : durée' Type='Text' Required='FALSE' ID='8405C27B-7148-48FB-9CC4-EA08370ABDF3'  StaticName='Col_CtrMat' Name='Col_CtrMatDuree'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrMatAgence"; value = "<Field DisplayName='Matériel : agence' Type='Text' Required='FALSE' ID='24D8191D-D811-48B2-BEA9-833BA880B2C1'  StaticName='Col_CtrMatAgence' Name='Col_CtrMatAgence'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrMatMission"; value = "<Field DisplayName='Matériel : missions' Type='Note' Required='FALSE' ID='A32192E7-EE9E-4F1E-AC54-CDC6C97D9272' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrMatMission' Name='Col_CtrMatMission'  Group='VC' />" }
        
        [pscustomobject]@{ key = "Col_CtrPointsForts"; value = "<Field DisplayName='Points forts' Type='Note' Required='FALSE' ID='F015E343-AE73-413D-A1F5-B6BF5F0C119B' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrPointsForts' Name='Col_CtrPointsForts'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrAxeProgress"; value = "<Field DisplayName='Axe progrès' Type='Note' Required='FALSE' ID='BB08369B-C977-4271-9FE6-22ECB00BE320' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrAxeProgress' Name='Col_CtrAxeProgress'  Group='VC' />" }

        [pscustomobject]@{ key = "Col_CtrFinComment"; value = "<Field DisplayName='Financier : commentaire' Type='Note' Required='FALSE' ID='F5FDB2FD-1E13-4610-8722-F5E72A76B9C3' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrFinComment' Name='Col_CtrFinComment'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrContractComment"; value = "<Field DisplayName='Contractuel : commentaire' Type='Note' Required='FALSE' ID='691CC283-F49D-46A5-AEE6-2ECFF8797C1E' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrContractComment' Name='Col_CtrContractComment'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrPrevComment"; value = "<Field DisplayName='Prévention : commentaire' Type='Note' Required='FALSE' ID='91A52FF2-DC4A-45F2-96BE-D6D9906C87CC' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrPrevComment' Name='Col_CtrPrevComment'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrCommerceComment"; value = "<Field DisplayName='Commencial : commentaire' Type='Note' Required='FALSE' ID='8804E530-34A8-4DAC-9DE4-4B31AD820DEA' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrCommerceComment' Name='Col_CtrCommerceComment'  Group='VC' />" }

        [pscustomobject]@{ key = "Col_CtrBilan"; value = "<Field DisplayName='Bilan' Type='Note' Required='FALSE' ID='A7386176-BC15-45DD-8CBB-05680BB09A86' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrBilan' Name='Col_CtrBilan'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrRemarksEvent"; value = "<Field DisplayName='Remarques éventuelles' Type='Note' Required='FALSE' ID='B847BB12-F04C-4A2E-AB54-6516059C0FC2' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrRemarksEvent' Name='Col_CtrRemarksEvent'  Group='VC' />" }

        [pscustomobject]@{ key = "Col_CtrPlusPlu"; value = "<Field DisplayName='Plus plu' Type='Note' Required='FALSE' ID='A489F86C-4983-4A3E-BCB3-514B265474C3' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrPlusPlu' Name='Col_CtrPlusPlu'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrMoinsPlu"; value = "<Field DisplayName='Moins plu' Type='Note' Required='FALSE' ID='D911E415-2946-4DEA-B423-1C3E8BC8A3E9' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrMoinsPlu' Name='Col_CtrMoinsPlu'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrFormationETF"; value = "<Field DisplayName='Formation ETF Ac.' Type='Note' Required='FALSE' ID='9C36F954-A099-4E70-8C27-C69F2B8F62C3' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrFormationETF' Name='Col_CtrFormationETF'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrDispoTut"; value = "<Field DisplayName='Disponibilité tuteur' Type='Note' Required='FALSE' ID='F3295156-DEC8-4E37-BCEA-D0D7B50CC176' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrDispoTut' Name='Col_CtrDispoTut'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrRessenti"; value = "<Field DisplayName='Ressenti' Type='Note' Required='FALSE' ID='BC1BA687-D562-4B90-B15C-53779FFA4A38' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrRessenti' Name='Col_CtrRessenti'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrLienPromo"; value = "<Field DisplayName='Lien promo' Type='Note' Required='FALSE' ID='5072A39F-8EE0-4E40-815B-10A7EEAD73B6' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrLienPromo' Name='Col_CtrLienPromo'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrPointsPos"; value = "<Field DisplayName='Points positifs' Type='Note' Required='FALSE' ID='E12565B0-B575-408A-B44B-B1A2CBC65B26' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrPointsPos' Name='Col_CtrPointsPos'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrIdeeAm"; value = "<Field DisplayName='Idées amélioration' Type='Note' Required='FALSE' ID='0E52BF09-4529-4F68-B7FE-F1F61AEF381C' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrIdeeAm' Name='Col_CtrIdeeAm'  Group='VC' />" }

        #Conducteurs 
        [pscustomobject]@{ key = "Col_CtrBilanPremSem"; value = "<Field DisplayName='Bilan premières semaines agence' Type='Note' Required='FALSE' ID='48999F0F-AA5E-447B-9467-1A57277B0EEA' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrBilanPremSem' Name='Col_CtrBilanPremSem'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrAspPratique"; value = "<Field DisplayName='Aspect pratique' Type='Note' Required='FALSE' ID='78BE05FF-7B97-4EAF-A9FA-EAC8D22B7EE6' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrAspPratique' Name='Col_CtrAspPratique'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_CtrDiffRenc"; value = "<Field DisplayName='Difficultés rencontrées' Type='Note' Required='FALSE' ID='BCE886FA-A181-4745-86A9-C2D581A864DC' UnlimitedLengthInDocumentLibrary='TRUE' StaticName='Col_CtrDiffRenc' Name='Col_CtrDiffRenc'  Group='VC' />" }


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
        [pscustomobject]@{ key = "Col_Order"; value = "<Field DisplayName='Ordre' Type='Number' Required='FALSE' ID='4d8086a0-0018-4261-9efe-ed778cc65d1d'  StaticName='Col_Order' Name='Col_Order'  Group='VC' />" }
    
        [pscustomobject]@{ key = "Col_E_Code"; value = "<Field DisplayName='Code' Type='Text' Required='FALSE' ID='7ae8799c-ccbe-4178-af8a-847b89038bcd'  StaticName='Col_E_Code' Name='Col_E_Code'  Group='VC' />" }
        [pscustomobject]@{ key = "Col_ParentId"; value = "<Field DisplayName='Parent Id' Type='Number' Required='FALSE' ID='d82d8d65-af2a-4451-ad88-b65fcc4b140b'  StaticName='Col_ParentId' Name='Col_ParentId'  Group='VC' Indexed='TRUE'/>" }
        
        #Access fields

        [pscustomobject]@{ key = "Col_UserRole"; value = "<Field Type='Choice' DisplayName='Rôle utilisateur' ID='8da866a3-d42b-4b22-b044-fcfac1c93ad6' Group='VC' Name='Col_UserRole' StaticName='Col_UserRole'  Required='false' FillInChoice='FALSE'  ><Default>Aiguilleur</Default> <CHOICES><CHOICE>Admin</CHOICE><CHOICE>RH</CHOICE><CHOICE>Aiguilleur</CHOICE><CHOICE>User</CHOICE></CHOICES></Field>" }
        [pscustomobject]@{ key = "Col_LevelRole"; value = "<Field Type='Text' DisplayName='Niveau accès' Name='Col_LevelRole'  StaticName='Col_LevelRole' ID='7c884426-31f0-4f5c-98c0-87a3b9181900' Group='VC' Required='false'  ></Field>" }
        [pscustomobject]@{ key = "Col_User"; value = "<Field Type='User' DisplayName='Utilisateur' Name='Col_User'  StaticName='Col_User' ID='4f299631-f8d7-4fff-84b7-27fa1bc6b31c' Group='VC' Required='false'   />" }
        

        # Aigilleurs List
        # [pscustomobject]@{ key = "Col_Function"; value = "<Field DisplayName='Fonction' Type='Text' Required='FALSE' ID='EA16A241-6647-4C4F-B099-F25D8D4FF0E6'  StaticName='Col_Function' Name='Col_Function'  Group='VC' />" }
        # [pscustomobject]@{ key = "Col_BusinessBu"; value = "<Field DisplayName='Filière métier' Type='Text' Required='FALSE' ID='52381EA9-C769-44C2-A4F8-CFFE4ECC8573'  StaticName='Col_BusinessBu' Name='Col_BusinessBu'  Group='VC' />" }
        # [pscustomobject]@{ key = "Col_TelPro"; value = "<Field DisplayName='Téléphone professionnel' Type='Text' Required='FALSE' ID='E5A7BB29-8579-4943-AF63-BB6C4D92F3ED'  StaticName='Col_TelPro' Name='Col_TelPro'  Group='VC' />" }
      

        
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
function CreateLkFields($ctx, $web) {
    $fieldsToCreate = @()
    $list = GetListByUrl $web "Lists/etf_interview" 
    $fieldsToCreate += InitColumn "Col_Lk_Request" "Fiche AG" "" "Lookup" "false" "   EnforceUniqueValues='FALSE'" $list.Id "Title"
    $list = GetListByUrl $web "Lists/etf_contract" 
    $fieldsToCreate += InitColumn "Col_Lk_Contract" "Fiche CT" "" "Lookup" "false" "  EnforceUniqueValues='FALSE'" $list.Id "Title"
   
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
    
    $ct = "ETF Mail template"
    AddContentType $ct
    $fields = LinkFields
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
    return $fields;
} 
function LinkCT() {
    $ct = "ETF Link"
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
    $ct = "ETF Settings"
    AddContentType $ct
    $fields = AppSettingsFields 
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
}
function OrganizationFields() {
    "Title"
    "Col_E_Code"
}
function OrganizationCT() {
    $ct = "ETF Organisation"
    AddContentType $ct
    $fields = OrganizationFields 
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
}
function BuFields() {
    "Col_E_Code"
}
function BuCT() {
    $ct = "ETF BU"
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
    
    $ct = "ETF Mail template"
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
        "Col_Comment"
        "Col_Author"
        "Col_Editor"
    )
    return $fields;
}
function AttachmentsCT($addParentFields) {
    $ct = "ETF Documents"
    AddContentType $ct "Document"
    $fields = AttachmentFields
    if ($addParentFields) {
        $fields += ParentFields
        $fields += CTParentFields
    }
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
}

function RootRequestFields() {
    $data = @(
        "Col_FormType"
        "Col_FormTarget"
        "Col_Status"
        "Col_UrlR"
        "Col_Bu"
        "Col_DirGeneral"
        "Col_FullName"
        "Col_Tel"
        "Col_Email"
        "Col_Position"
        "Col_RespUser"
        "Col_RespFullName"
        "Col_RespPosition"
        "Col_RespUserSPId"
        "Col_StartDate"
        "Col_EndDate"
        "Col_Guid"
        "Col_Author"
        "Col_Editor"
        "Col_RespSignture"
        "Col_RespSignName"
        "Col_RespSignDate"
        "Col_CandSignture"
        "Col_CandSignName"
        "Col_CandSignDate"
       
    )
    return $data;
}

function InterviewFields() {
    $data = @(
        "Col_StartDateT"
        "Col_RespUser2"
        "Col_RespUserSPId2"
        "Col_RespFullName2"
        "Col_DurationM"
        "Col_Participants"
        "Col_Ecoute"
        "Col_QualityExchange"
        "Col_Autonomy"
        "Col_EvAigComment"
        "Col_EvFlComment"
        "Col_BlAigComment"
        "Col_BlFlComment"
        "Col_Lesson"
    )
    return $data;
}
function StageFields() {
    $data = @(
        "Col_DueDate"
        "Col_CtrMeetDate"
        "Col_CtrEcole"
        "Col_CtrDiplome"
        "Col_DurationM"
        "Col_CtrDescMission"
        "Col_CtrComp"
        "Col_CtrTypeCh"
        "Col_CtrPositionETF"
        "Col_CtrPositionETFDesc"
        "Col_CtrMobility"
        "Col_CtrMobilityReg"
        "Col_CtrPojectPro"
        "Col_CtrRecoETF"
        "Col_CtrRecoEtud"
        "Col_CtrEmbaucheETF"
        "Col_CtrObs"
        "Col_Order"
        "Col_CtrRootReq"
       
    )
    return $data;
}
function ChantierFields() {
    $data = @(
        "Col_DueDate"
        "Col_CtrHiringDate"
        "Col_Participants"
        "Col_CtrDescMission"
        "Col_CtrIntAgence"
        "Col_CtrIntPromo"
        "Col_CtrPtPos"
        "Col_CtrObs"
        "Col_CtrlAccComment"
        "Col_CtrSavFComment"
        "Col_CtrSupComment"
        "Col_CtrProgComment"
        "Col_CtrFormComment"
        "Col_CtrSecFer"
        "Col_CtrSecFerDuree"
        "Col_CtrSecFerAgence"
        "Col_CtrSecFerMission"
        "Col_CtrPrev"
        "Col_CtrPrevDuree"
        "Col_CtrPrevAgence"
        "Col_CtrPrevMission"
        "Col_CtrMat"
        "Col_CtrMatDuree"
        "Col_CtrMatAgence"
        "Col_CtrMatMission"
        "Col_CtrPointsForts"
        "Col_CtrAxeProgress"
        "Col_CtrFinComment"
        "Col_CtrContractComment"
        "Col_CtrPrevComment"
        "Col_CtrCommerceComment"
        "Col_CtrBilan"
        "Col_CtrRemarksEvent"
        "Col_CtrPlusPlu"
        "Col_CtrMoinsPlu"
        "Col_CtrFormationETF"
        "Col_CtrDispoTut"
        "Col_CtrRessenti"
        "Col_CtrLienPromo"
        "Col_CtrPointsPos"
        "Col_CtrIdeeAm"
        "Col_Order"
        "Col_CtrRootReq"


    )
    return $data;
}
function ConducteurFields() {
    $data = @(
        "Col_DueDate"
        "Col_CtrHiringDate"
        "Col_CtrMeetDate"
        "Col_Position"
        "Col_Promotion"
        "Col_Participants"
        "Col_CtrBilanPremSem"
        "Col_CtrAspPratique"
        "Col_CtrIntPromo"
        "Col_CtrDiffRenc"
        "Col_CtrDispoTut"
        "Col_CtrRemarksEvent"
        "Col_CtrDescMission"
        "Col_CtrSecFer"
        "Col_CtrSecFerMission"
        "Col_CtrPrev"
        "Col_CtrPrevMission"
        "Col_CtrMat"
        "Col_CtrMatMission"
        "Col_CtrFinComment"
        "Col_CtrContractComment"
        "Col_CtrPrevComment"
        "Col_CtrCommerceComment"
        "Col_CtrPointsForts"
        "Col_CtrAxeProgress"
        "Col_CtrBilan"
        "Col_Order"
        "Col_CtrRootReq"

        
    )
    return $data;
}
function ConducteurCT($addParentFields) {
    $ct = "ETF Conducteur"
    AddContentType $ct "ETF Root Request"
    $fields = ConducteurFields
    if ($addParentFields) {
        $fields += CTParentFields
    }
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
    return $fields;
}
function RootRequestCT() {
    $ct = "ETF Root Request"
    AddContentType $ct
    $fields = RootRequestFields
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
    return $fields;
}
function interviewCT() {
    $ct = "ETF Interview" 
    AddContentType $ct "ETF Root Request"
    $fields = InterviewFields
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
    return $fields;
}
function stageCT($addParentFields) {
    $ct = "ETF Stage" 
    AddContentType $ct "ETF Root Request"
    $fields = StageFields
    if ($addParentFields) {
        $fields += CTParentFields
    }
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
    return $fields;
}
function ChantierCT($addParentFields) {
    $ct = "ETF Chef chantier" 
    AddContentType $ct "ETF Root Request"
    $fields = ChantierFields
    if ($addParentFields) {
        $fields += CTParentFields
    }
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
    return $fields;
}
function QuartlyInterviewFields() {
    $data = @(
        "Col_DueDate"    
        "Col_CtrMeetDate"
        "Col_Status"
        "Col_MomMissing"
        "Col_Success"
        "Col_SoftSkills"
        "Col_CompSec"
        "Col_Guid"
        "Col_Order"
        "Col_Author"
        "Col_Editor"
    )
    return $data;
}
function QuartlyInterviewCT($addParentFields) {
    $ct = "ETF Quartly interview"
    AddContentType $ct
    $fields = QuartlyInterviewFields
    if ($addParentFields) {
        $fields += ParentFields
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
function RelatedInterviewCT($addParentFields) {
    $ct = "ETF Related interview"
    AddContentType $ct
    $fields = RelatedInterviewFields
    if ($addParentFields) {
        $fields += ParentFields
    }
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
    return $fields;
}
function RelatedActionFields() {
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
function RelatedActionCT($addParentFields) {
    $ct = "ETF Related action"
    AddContentType $ct
    $fields = RelatedActionFields
    if ($addParentFields) {
        $fields += CTParentFields
    }
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
    return $fields;
}
function TemplateActionFields() {
    $fields = @(
        "Col_Action1"
        "Col_Action2"
        "Col_Comment"
        "Col_Guid"
        "Col_Order"
        "Col_Author"
        "Col_Editor"
    )
    return $fields;
}
function TemplateActionCT() {
    $ct = "ETF Template action"
    AddContentType $ct
    $fields = TemplateActionFields
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
    return $fields;
}
function TemplateActionNewList($web, $addSiteFields) {

    $url = "etf_tempaction"
    $list = "App - Modèles actions"
    $ct = "ETF Template action"
    $fields = TemplateActionFields
    $fields = @('Title') + $fields
    CreateListV2 $web $list $url $listDesc "GenericList"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    UpdateListView -List $l.Id -Fields $fields
}
function AttachmentsAGNewList($web, $addSiteFields) {

    $url = "etf_agattachments"
    $list = "AG - Documents"
    $ct = "ETF Documents"
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
function AttachmentsCTNewList($web, $addSiteFields) {

    $url = "etf_ctattachments"
    $list = "CT - Documents"
    $ct = "ETF Documents"
    $fields = AttachmentFields
    if ($addSiteFields) {
        $fields += CTParentFields
    }
    CreateListV2 $web $list $url $listDesc "DocumentLibrary"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    UpdateListView -List $l.Id -Fields $fields
    AddFieldIndex  $l.Id "Col_ParentId"
    AddFieldIndex  $l.Id "Col_Lk_Contract"
    AddFieldIndex  $l.Id "Created"
    AddFieldIndex  $l.Id "Modified"

}
function TemplateNewList($web) {

    $url = "etf_templates"
    $list = "App - Templates"
    $ct = "ETF Documents"
    $fields = AttachmentFields
    CreateListV2 $web $list $url $listDesc "DocumentLibrary"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    UpdateListView -List $l.Id -Fields $fields
    AddFieldIndex  $l.Id "Created"
    AddFieldIndex  $l.Id "Modified"

}
function ParentFields() {
    $fields = @(
        "Col_Lk_Request"
        "Col_Lk_Contract"
    )
    return $fields;
}
function CTParentFields() {
    $fields = @(
        "Col_Lk_Contract"
    )
    return $fields;
}
function InterviewList($web, $context) {
    $url = "Lists/etf_interview"
    $list = "AG - Carnets de bord"
    $ct = "ETF Interview"
    $fields = InterviewFields 
    CreateListV2 $web $list $url $listDesc "GenericList"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    $viewFields = getViewFields "AG"
    UpdateListView -List $l.Id -Fields $viewFields
    $viewFields = getViewFields "AGAll"
    AddListView2 -ctx $context -List $l.Id -Title "Tableau"  -Fields $viewFields -Query "<OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"
    AddFieldIndex  $l.Id "Col_RespUser"
    AddFieldIndex  $l.Id "Col_Status"
    AddFieldIndex  $l.Id "Col_StartDateT"
    AddFieldIndex  $l.Id "Col_StartDate"
    AddFieldIndex  $l.Id "Col_Author"
    AddFieldIndex  $l.Id "Col_Editor"

}
function RelatedInterviewList($web, $addSiteFields) {
    $url = "Lists/etf_relatedinterview"
    $list = "AG - Actions"
    $ct = "ETF Related interview"
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
    AddFieldIndex  $l.Id "Author"
    AddFieldIndex  $l.Id "Editor"

}
function QuartlyInterviewList($web, $addSiteFields) {
    $url = "Lists/etf_quartlyinterview"
    $list = "AG - Entretiens trimestiels"
    $ct = "ETF Quartly interview"
    $fields = QuartlyInterviewFields
    if ($addSiteFields) {
        $fields += ParentFields
    }
    $l = GetListByUrl $web "$url"
    CreateListV2 $web $list $url $listDesc "GenericList"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    $viewFields = getViewFields "QINT"
    UpdateListView -List $l.Id -Fields $viewFields
    AddFieldIndex  $l.Id "Col_Lk_Request"
    AddFieldIndex  $l.Id "Created"
    AddFieldIndex  $l.Id "Modified"

}
function ContractNewList($web, $context) {
    $url = "Lists/etf_contract"
    $list = "CT - Contrats"
    CreateListV2 $web $list $url $listDesc "GenericList"
  
    $l = GetListByUrl $web "$url"
    if ($l.id -eq $null) {
        Start-Sleep -Milliseconds 2000
        $l = GetListByUrl $web "$url"
        Start-Sleep -Milliseconds 2000
    }
    $ct = "ETF Stage"
    AddCTToListV2 $l.Id $ct
    $ct = "ETF Chef chantier"
    AddCTToListV2 $l.Id $ct
    $ct = "ETF Conducteur"
    AddCTToListV2 $l.Id $ct
    $viewFields = getViewFields "CTALL"
    UpdateListView -List $l.Id -Fields $viewFields
    $viewFields = getViewFields "CTStage"
    AddListView2 -ctx $context -List $l.Id -Title "Stage"  -Fields $viewFields -Query "<Where><Eq><FieldRef Name='Col_FormTarget'/><Value Type='Text'>Stage</Value></Eq></Where><OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"

    # AddListView2 -ctx $context -List $l.Id -Title "Stage - Etudiant"  -Fields $viewFields -Query "<Where><Eq><FieldRef Name='Col_FormType'/><Value Type='Text'>Stage - Etudiant</Value></Eq></Where><OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"
    # AddListView2 -ctx $context -List $l.Id -Title "Stage - Tuteur"  -Fields $viewFields -Query "<Where><Eq><FieldRef Name='Col_FormType'/><Value Type='Text'>Stage - Tuteur</Value></Eq></Where><OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"
    # AddListView2 -ctx $context -List $l.Id -Title "Alternance - Etudiant"  -Fields $viewFields -Query "<Where><Eq><FieldRef Name='Col_FormType'/><Value Type='Text'>Alternance - Etudiant</Value></Eq></Where><OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"
    # AddListView2 -ctx $context -List $l.Id -Title "Alternance - Tuteur"  -Fields $viewFields -Query "<Where><Eq><FieldRef Name='Col_FormType'/><Value Type='Text'>Alternance - Tuteur</Value></Eq></Where><OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"
    $viewFields = getViewFields "CTChantier"
    AddListView2 -ctx $context -List $l.Id -Title "Chantier"  -Fields $viewFields -Query "<Where><Eq><FieldRef Name='Col_FormTarget'/><Value Type='Text'>Chantier</Value></Eq></Where><OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"
    # AddListView2 -ctx $context -List $l.Id -Title "Chantier - Intégration"  -Fields $viewFields -Query "<Where><Eq><FieldRef Name='Col_FormType'/><Value Type='Text'>Chantier</Value></Eq></Where><OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"
    # $viewFields = getViewFields "CTChantier"
    # AddListView2 -ctx $context -List $l.Id -Title "Chantier - Tuteur - Graduate"  -Fields $viewFields -Query "<Where><Eq><FieldRef Name='Col_FormType'/><Value Type='Text'>Chantier</Value></Eq></Where><OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"
    # $viewFields = getViewFields "CTChantier"
    # AddListView2 -ctx $context -List $l.Id -Title "Chantier - RH – Graduate"  -Fields $viewFields -Query "<Where><Eq><FieldRef Name='Col_FormType'/><Value Type='Text'>Chantier</Value></Eq></Where><OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"
    
    $viewFields = getViewFields "CTConducteur"
    AddListView2 -ctx $context -List $l.Id -Title "Conducteur"  -Fields $viewFields -Query "<Where><Eq><FieldRef Name='Col_FormTarget'/><Value Type='Text'>Conducteur</Value></Eq></Where><OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"

    # AddListView2 -ctx $context -List $l.Id -Title "CT - Bilan d'intégration à 1 mois"  -Fields $viewFields -Query "<Where><Eq><FieldRef Name='Col_FormType'/><Value Type='Text'>CT - Bilan d'intégration à 1 mois</Value></Eq></Where><OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"
    # $viewFields = getViewFields "CTConducteur"
    # AddListView2 -ctx $context -List $l.Id -Title "CT - Bilan 1ère affectation"  -Fields $viewFields -Query "<Where><Eq><FieldRef Name='Col_FormType'/><Value Type='Text'>CT - Bilan 1ère affectation</Value></Eq></Where><OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"
    # $viewFields = getViewFields "CTConducteur"
    # AddListView2 -ctx $context -List $l.Id -Title "CT - Bilan 2nd affectation"  -Fields $viewFields -Query "<Where><Eq><FieldRef Name='Col_FormType'/><Value Type='Text'>CT - Bilan 2nd affectation</Value></Eq></Where><OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"
    # $viewFields = getViewFields "CTConducteur"
    # AddListView2 -ctx $context -List $l.Id -Title "CT - Bilan RH 1ère affectation"  -Fields $viewFields -Query "<Where><Eq><FieldRef Name='Col_FormType'/><Value Type='Text'>CT - Bilan RH 1ère affectation</Value></Eq></Where><OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"
    # $viewFields = getViewFields "CTConducteur"
    # AddListView2 -ctx $context -List $l.Id -Title "CT - Bilan RH 2nd affectation"  -Fields $viewFields -Query "<Where><Eq><FieldRef Name='Col_FormType'/><Value Type='Text'>CT - Bilan RH 2nd affectation</Value></Eq></Where><OrderBy><FieldRef Name='ID'  Ascending='FALSE'/></OrderBy>"

    AddFieldIndex  $l.Id "Col_Status"
    AddFieldIndex  $l.Id "Col_FormType"
    AddFieldIndex  $l.Id "Col_FormTarget"
    AddFieldIndex  $l.Id "Col_CtrRootReq"
    AddFieldIndex  $l.Id "Col_Author"
    AddFieldIndex  $l.Id "Col_Editor"

}
function RelatedContractsNewList($web, $addSiteFields) {
    $url = "Lists/etf_relatedcontract"
    $list = "CT - Actions"
    $ct = "ETF Related action"
    $fields = RelatedActionFields 
    if ($addSiteFields) {
        $fields += CTParentFields
    }
    $l = GetListByUrl $web "$url"
    CreateListV2 $web $list $url $listDesc "GenericList"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    UpdateListView -List $l.Id -Fields $fields
    AddFieldIndex  $l.Id "Col_Lk_Contract"
    AddFieldIndex  $l.Id "Created"
    AddFieldIndex  $l.Id "Modified"
    AddFieldIndex  $l.Id "Title"
    AddFieldIndex  $l.Id "Author"
    AddFieldIndex  $l.Id "Editor"

}
function getViewFields($target) {
    $r = @()
    switch ($target) {
        "QINT" {
            $r = @(
                "LinkTitle"
                "Col_DueDate"    
                "Col_CtrMeetDate"
                "Col_Status"
                "Col_Author"
                "Col_Editor"
                "Col_Order"
        
            )
        }
        "AG" {
            $r = @(
                "ID"
                "LinkTitle"
                "Col_RespUser"
                "Col_RespUser2"
                "Col_FullName"
                "Col_DirGeneral"
                "Col_Bu"
                "Col_Status"
                "Col_UrlR"
                "Created"
                "Col_StartDateT"
                "Col_DurationM"
                "Col_StartDate"
                "Col_EndDate"
                "Col_Participants"
       
                "Modified"
                "Col_Author"
                "Col_Editor"
            )  
        }
        "AGAll" { 
            $r = @(
                "ID"
                "LinkTitle"
                "Col_FormType"
                "Col_FormTarget"
                "Col_UrlR"
                "Col_Status"
                "Col_DirGeneral"
                "Col_Bu"
                "Col_FullName"
                "Col_Tel"
                "Col_Email"
                "Col_Position"
                "Col_RespUser"
                "Col_RespFullName"
                "Col_RespUserSPId"
                "Col_StartDateT"
                "Col_StartDate"
                "Col_EndDate"
                "Col_DurationM"
                "Col_RespUser2"
                "Col_RespUserSPId2"
                "Col_RespFullName2"
                "Col_Participants"
                "Col_Ecoute"
                "Col_QualityExchange"
                "Col_Autonomy"
                "Col_EvAigComment"
                "Col_EvFlComment"
                "Col_BlAigComment"
                "Col_BlFlComment"
                "Col_Lesson"
                "Col_Author"
                "Col_Editor"
                "Modified"
                "Col_Guid"
                "Col_RespSignture"
                "Col_RespSignName"
                "Col_RespSignDate"
                "Col_CandSignture"
                "Col_CandSignName"
                "Col_CandSignDate"
                
            ) 
        }
        "CTStage" {  
                
            $r = @(
                "ID"
                "LinkTitle"
                "Col_FormType"
                "Col_DirGeneral"
                "Col_Bu"
                "Col_UrlR"
                "Col_Status"
                "Col_CtrMeetDate"
                "Col_FullName"
                "Col_Email"
                "Col_CtrDiplome"
                "Col_Position"
                "Col_RespFullName"
                "Col_RespPosition"
                "Col_StartDate"
                "Col_EndDate"
                "Col_CtrDescMission"
                "Col_Author"
                "Col_Editor"
                "Created"
                "Modified"
            ) 
        }
        "CTChantier" {
            $r = @(
                "ID"
                "LinkTitle"
                "Col_FormType"
                "Col_DirGeneral"
                "Col_Bu"
                "Col_UrlR"
                "Col_RespUser"
                "Col_FullName"
                "Col_Status"
                "Col_StartDate"
                "Col_EndDate"
                "Col_CtrHiringDate"
                "Col_Participants"
                "Col_CtrDescMission"
                "Col_Author"
                "Col_Editor"
                "Created"
                "Modified"
            )
        }
        "CTConducteur" {
            $r = @(
                "ID"
                "LinkTitle"
                "Col_FormType"
                "Col_DirGeneral"
                "Col_Bu"
                "Col_RespFullName"
                "Col_FullName"
                "Col_Position"
                "Col_Promotion"
                "Col_Status"
                "Col_CtrHiringDate"
                "Col_CtrMeetDate"
                "Col_Participants"
                "Col_Author"
                "Col_Editor"
                "Created"
                "Modified"
            )
        }
        "CTALL" {  
            $r = @(
                "ID"
                "LinkTitle"
                "Col_FormType"
                "Col_FormTarget"
                "Col_DirGeneral"
                "Col_Bu"
                "Col_Status"
                "Col_UrlR"
                "Col_FullName"
                "Col_Position"
                "Col_RespFullName"
                "Col_RespPosition"
                "Col_CtrMeetDate"
                "Col_StartDate"
                "Col_EndDate"
                "Col_CtrHiringDate"
                "Col_Author"
                "Col_Editor"
                "Created"
                "Modified"
            ) 
        }
        Default {}
    }
    return $r;
}

function MailTemplateList($web) {
    $url = "Lists/etf_mailtemplate"
    $list = "App - Mail templates"
    $ct = "ETF Mail template"
    $fields = MailTemplateFields 
    CreateListV2 $web $list $url $listDesc "GenericList"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    UpdateListView -List $l.Id -Fields $fields
}
function AppSettingsNewList($web) {
    $url = "Lists/etf_settings"
    $list = "App - Paramètres"
    $ct = "ETF Settings"
    CreateListV2 $web $list $url $listDesc "GenericList"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    $fields = AppSettingsFields
    UpdateListView -List $l.Id -Fields $fields
    "Created"
}

function OrganisationNewList($web) {
    $url = "Lists/etf_organization"
    $list = "App - Organisation"
    $ct = "ETF Organisation"
    CreateListV2 $web $list $url $listDesc "GenericList"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    $fields = OrganizationFields
    UpdateListView -List $l.Id -Fields $fields
}
function BuNewList($web) {
    $url = "Lists/etf_Bu"
    $list = "App - Agences"
    $ct = "ETF BU"
    CreateListV2 $web $list $url $listDesc "GenericList"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    $fields = AppSettingsFields
    UpdateListView -List $l.Id -Fields $fields
}
function LinkNewList($web) {
    $url = "Lists/etf_link"
    $list = "App - Liens"
    $ct = "ETF Link"
    $fields = LinkFields 
    CreateListV2 $web $list $url $listDesc "GenericList"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    UpdateListView -List $l.Id -Fields $fields
}
function AccessFields() {
    $fields = @(
        "Col_User"
        "Col_E_Code"
        "Col_UserRole"
        "Col_LevelRole"
        "Col_Author"
        "Col_Editor"
    )
    return $fields;
}
function AccessCT() {
    $ct = "ETF Access"
    AddContentType $ct
    $fields = AccessFields
    $fields | ForEach-Object {
        $staticName = $_
        AddFieldToCT $staticName $ct
    }
}
function AccessCTNewList($web) {
    $url = "Lists/etf_ctaccess"
    $list = "CT - Accès"
    $ct = "ETF Access"
    $fields = AccessFields 
    CreateListV2 $web $list $url $listDesc "GenericList"
    $l = GetListByUrl $web "$url"
    AddCTToListV2 $l.Id $ct
    UpdateListView -List $l.Id -Fields $fields
}
function AccessAGNewList($web) {
    $url = "Lists/etf_agaccess"
    $list = "AG - Accès"
    $ct = "ETF Access"
    $fields = AccessFields 
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
    # RootRequestCT
    # InterviewCT
    # stageCT
    # ChantierCT
    # ConducteurCT
    # RelatedInterviewCT $false
    # RelatedActionCT  $false
    # QuartlyInterviewCT $false
    # AttachmentsCT $false
    # LinkCT
    # MailTemplateCT
    # AppSettingsCT
    # OrganizationCT
    # BuCT
    # AccessCT
    # TemplateActionCT
 
    # InterviewList $web $context
    ContractNewList $web $context
    # CreateLkFields  $context $web
    # MailTemplateList $web
    # LinkNewList $web
    # OrganisationNewList $web
    # AppSettingsNewList $web
    # BuNewList $web
    # RelatedInterviewCT $true
    # QuartlyInterviewCT $true
    # QuartlyInterviewList $web
    # RelatedActionCT $true
    # RelatedContractsNewList $web $true
    # RelatedInterviewList $web
    # AttachmentsCT $true
    # AttachmentsAGNewList $web $true
    # AttachmentsCTNewList $web $true
    TemplateNewList $web
    # AccessCTNewList $web
    # AccessAGNewList $web
    # TemplateActionNewList $web
    # stageCT $true
    # ChantierCT $true
    # ConducteurCT $true

}
$siteURL = "https://vincic.sharepoint.com/sites/etag-dev4"
#$siteURL = "https://futur365.sharepoint.com/sites/Dev08"

cls
LoadDependency
$newInstall = $true
SetStructureRoot $siteURL