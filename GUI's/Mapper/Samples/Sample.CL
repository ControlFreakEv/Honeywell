--!!!mapper sample package!!!
--program auto indents ("Pretty Print" button) when file is opened (click "Original CL" to see the file before it has been formatted)
--click this line, then click the "Uncomment"/"Comment" buttons
--dobule click the word in quotes "parameter" and see how it highlights all similar words
--dobule click the word in quotes "parameter", then ctrl+f or click the "Find" button. Check the box "Mark Line", then click "Find All"
--Pick a color next to "Highlight", select text, then press "Highlight" button. "Save" button will save highlights
--"Modified CL" will resolve TDC2000 indirect references e.g. box parameters like LP(01), FL(02), NN(3), will be replaced by a tag defined in EB files if they exists, otherwise, tags will have a prefix such as "$HWY1BOX2."

PACKAGE --key/reservered words appear pink
	
ENUMERATION TEST_ENUM = NORMAL/BYPASS --enumerations are green. Hover over TEST_ENUM
	
PARAM_LIST TEST_PARAMLIST --param lists are green. hover over TEST_PARAMLIST
PARAMETER PV1:TEST_ENUM --custom data segment are blue
PARAMETER PV2:TEST_ENUM -- hover over PV2
END TEST_PARAMLIST
	
CUSTOM ( ACCESS ENGINEER; BLD_VISIBLE)
		
PARAMETER MAINT_Y,MAINT_T: NUMBER ARRAY(1..100) "DAILY MAINTENANCE IN MINS" --hover over MAINT_Y or MAINT_T
VALUE 0.0 --numbers are green
NOT BLD_VISIBLE
		
PARAMETER STATUS:STRING ARRAY(1..100) --hover over STATUS
NOT BLD_VISIBLE
		
PARAMETER NUL_S:$REG_CTL --hover over NUL_S
VALUE SampleDICMPNIM --external tag defined below, click to show parameters
		
END CUSTOM
	
BLOCK SAMPLECL ( GENERIC; AT GENERAL) --click "Original CL"/"Modified CL" buttons to see package tag replacement. See "Package" dropdown, to change package tags/CDS
		
EXTERNAL SampleDIINNIM, SampleDICMPNIM --tags that exist in EB are cyan. Click on any external tag to see parameters
EXTERNAL MISSINGTAG --tags not found in EB will be red
		
LOCAL K,I:NUMBER AT NN(01) --local variables are orange, hover over to see type
		
		
%RELAX LINKER_SDE_CHECKS --directives appear gray
		
IF SampleDIINNIM.PV = "test" THEN --strings are yellow
& SET PV1 = (1.0/60.0)*52.3+(6**5-123) --space added to math operators (click "Original CL"/"Modified CL" buttons)
else
& set i = max(MAINT_Y) --built in functions appear khaki and have dotted line underneath. Hover over to see function description
		
ENDSHIFT: LOOP FOR I IN 1..100 --loop for statements appear teal
SET STATUS(I) = " status "
REPEAT ENDSHIFT
		
SET SampleDIINNIM.PV = STOP --white text is the default color for unformatted text. These are typically parameters or enum values e.g. "PV" and "STOP". 

END SAMPLECL --block labels appear teal
	
END PACKAGE
