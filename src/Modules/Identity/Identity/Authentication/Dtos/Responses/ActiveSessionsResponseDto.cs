using Identity.Authentication.Dtos.Common;

namespace Identity.Authentication.Dtos.Responses;

public record ActiveSessionsResponseDto(List<DeviceSessionDto> Sessions);
