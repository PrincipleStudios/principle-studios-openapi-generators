using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.TypeScript.Templates;

public record ModelBarrelFileTemplate(
	PartialHeader Header,

	ModelBarrelFile Model
);

public record ExportMember(
	string MemberName,
	bool IsType
);

public record ExportStatement(
	ExportMember[] Members,
	string Path
);

public record ModelBarrelFile(PartialHeader Header, ExportStatement[] Exports);
