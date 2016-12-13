%namespace Asap2
%partial
%parsertype Asap2Parser
%visibility internal
%tokentype Token

%union { 
			public int n;
			public string s;
			public ALIGNMENT_type alignment_token;
			public Tuple<ALIGNMENT_type, uint> alignment;
			public DEPOSIT deposit;
			public BYTE_ORDER byte_order;
			public Tuple<String, String> s2;
			public List<Tuple<String, String>> ls2;
	   }

%start main

%token <n> NUMBER
%token <s> QUOTED_STRING
%token <s> IDENTIFIER
%token <n> HEXNUMBER
%token ASAP2_VERSION
%token PROJECT
%token HEADER
%token MODULE
%token MOD_COMMON
%token DEPOSIT
%token BYTE_ORDER
%token DATA_SIZE
%token VERSION
%token PROJECT_NO
%token MEASUREMENT
%token CHARACTERISTIC
%token ECU_ADDRESS
%token ECU_ADDRESS_EXTENSION
%token FORMAT
%token <alignment_token> ALIGNMENT

%token BEGIN
%token END

%type <deposit>			deposit
%type <byte_order>		byte_order
%type <n>				data_size
%type <s>				project_no
%type <s>				version
%type <alignment>       alignment

%%

main	: project
		| asap2_version project
		| asap2_version project header
		| any
		| main any
		;

any    :  module
	   | measurement
	   | ecu_address
	   | format
	   | ecu_address_extension
	   | end_measurement
	   | end_module
       | end_project
       ;

asap2_version	:   ASAP2_VERSION NUMBER NUMBER {
                        Asap2File.asap2_version = new ASAP2_VERSION((uint)$2, (uint)$3);
                    }
                ;

project			:	BEGIN PROJECT IDENTIFIER QUOTED_STRING {
						Asap2File.project = new PROJECT($3, $4);
					}
				;

end_project		:	END PROJECT
				;

header			:	BEGIN HEADER QUOTED_STRING version project_no END HEADER {
						Asap2File.project.header = new HEADER(comment: $3, version: $4, project_no: $5);
					}
				|	BEGIN HEADER QUOTED_STRING project_no version END HEADER {
						Asap2File.project.header = new HEADER(comment: $3, project_no: $4, version: $5);
					}
				;

project_no		:	PROJECT_NO IDENTIFIER	{ $$ = $2; }
				|	PROJECT_NO NUMBER		{ $$ = $2.ToString(); }
				|	PROJECT_NO HEXNUMBER	{ $$ = $2.ToString(); }
				;

version			:	VERSION QUOTED_STRING	{ $$ = $2; }
				;

beg_module		:	BEGIN MODULE IDENTIFIER QUOTED_STRING {
						currentModule = new MODULE(name: $3, LongIdentifier: $4);
					}
				;

end_module		:	END MODULE {
						Asap2File.project.modules.Add(currentModule.name, currentModule);
						currentModule = null;
					}
				;

module			:	beg_module module_elements end_module
				;

module_elements : mod_common
				;

mod_common      : mod_common_begin mod_common_datas END MOD_COMMON
				;

mod_common_begin: BEGIN MOD_COMMON QUOTED_STRING {
					currentModule.mod_common = new MOD_COMMON($3);
				}
				;

mod_common_datas: /* empty */ {
                    }

				| mod_common_datas mod_common_data
				;

mod_common_data	:  deposit {
					Console.WriteLine("Found deposit");
					currentModule.mod_common.deposit    = $1;
				}
                |  byte_order {
					Console.WriteLine("Found byte_order");
					currentModule.mod_common.byte_order = $1;
				}
                |  data_size {
					Console.WriteLine("Found date_size");
					currentModule.mod_common.data_size  = (uint)$1;
				}
				|  alignment {
					Console.WriteLine("Found alignment");
					currentModule.mod_common.alignments.Add($1.Item1, $1.Item2);
				}
				;



measurement		:	BEGIN MEASUREMENT IDENTIFIER QUOTED_STRING IDENTIFIER IDENTIFIER NUMBER NUMBER NUMBER NUMBER {
						currentMeasurment = new MEASUREMENT($3, $4, $5, $6, (uint)$7, (uint)$8, (uint)$9, (uint)$10);
						currentModule.measurements.Add($3, currentMeasurment);
					}
				;

end_measurement	:	END MEASUREMENT {currentMeasurment = null;}
				;

alignment		:   ALIGNMENT NUMBER {
                        $$ = new Tuple<ALIGNMENT_type, uint>($1, (uint)$2);
                    }
                ;

deposit			: DEPOSIT IDENTIFIER {
							switch ($2)
							{
								case "ABSOLUTE":
									$$ = DEPOSIT.ABSOLUTE;
								break;
								case "DIFFERENCE":
									$$ = DEPOSIT.DIFFERENCE;
								break;
								default:
								throw new Exception("Unknown DEPOSIT type: " + $2);
							}
						}
                ;

byte_order		: BYTE_ORDER IDENTIFIER {
							switch ($2)
							{
								case "MSB_FIRST":
									$$ = BYTE_ORDER.MSB_FIRST;
								break;
								case "MSB_LAST":
									$$ = BYTE_ORDER.MSB_LAST;
								break;
								default:
								throw new Exception("Unknown BYTE_ORDER type: " + $2);
							}
						}
                ;

data_size		: DATA_SIZE HEXNUMBER
					{
                        $$ = $2;
                    }
				| DATA_SIZE NUMBER
					{
                        $$ = $2;
                    }
				;

ecu_address					: ECU_ADDRESS HEXNUMBER {SetEcuAddress((UInt64)$2);};
ecu_address_extension		: ECU_ADDRESS_EXTENSION HEXNUMBER {SetEcuAddressExtension((UInt64)$2);};
format						: FORMAT QUOTED_STRING {SetFormat($2);};

%%
private MODULE currentModule;
private MEASUREMENT currentMeasurment;
void SetEcuAddress(UInt64 Address)
{
	if (currentMeasurment != null)
	{
		currentMeasurment.ECU_ADDRESS = Address;
	}
}
void SetEcuAddressExtension(UInt64 Address)
{
	if (currentMeasurment != null)
	{
		currentMeasurment.ECU_ADDRESS_EXTENSION = Address;
	}
}
void SetFormat(string Format)
{
	if (currentMeasurment != null)
	{
		currentMeasurment.FORMAT = Format;
	}
}