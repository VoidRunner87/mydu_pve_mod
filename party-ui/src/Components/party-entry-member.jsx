import {ChevronUpIcon, DotIcon} from "./icons";
import {
    ConstructName,
    ConstructSize,
    FilledBar,
    GridColBars,
    GridColShipName,
    GridRow,
    GridRowBars,
    PercentageBar,
    PlayerName,
    Role,
    WidgetRow
} from "./widget";

const PartyEntryMember = ({item, onDoubleClick}) => {

    const getShieldColor = (value) => {
        if (value < 0.25) {
            return "rgb(250, 70, 70)";
        }

        return "#00FFFF";
    }

    const getCcsColor = (value) => {
        if (value < 0.25) {
            return "rgb(250, 70, 70)";
        }

        return "#E0D5AC";
    };

    const getConnectedColor = (value) => {
        if (value) {
            return "#00FF00";
        }

        return "#000000";
    };

    const getCoreSizeName = (size) => {
        switch (size) {
            case 32:
                return "XS";
            case 64:
                return "S";
            case 128:
                return "M";
            case 256:
                return "L";
            case 512:
                return "XL";
            default:
                return "??";
        }
    }

    const percentValue = (value) => value * 100;

    if (!item) {
        return null;
    }

    const ConstructRow = ({construct}) => {
        if (!construct) {
            return null;
        }

        return (
            <GridRow>
                <GridColShipName>
                    <ConstructSize>{getCoreSizeName(construct.Size)}</ConstructSize>
                    <ConstructName> - [789] "{construct.ConstructName}"</ConstructName>
                </GridColShipName>
                <GridColBars>
                    <GridRowBars>
                        <PercentageBar>
                            <FilledBar percentage={percentValue(construct.ShieldRatio)}
                                       color={getShieldColor(construct.ShieldRatio)}/>
                        </PercentageBar>
                    </GridRowBars>
                    <GridRowBars>
                        <PercentageBar>
                            <FilledBar percentage={percentValue(construct.CoreStressRatio)}
                                       color={getCcsColor(construct.CoreStressRatio)}/>
                        </PercentageBar>
                    </GridRowBars>
                </GridColBars>
            </GridRow>
        );
    }

    const handleDoubleClick = (playerId) => {
        if (onDoubleClick) {
            onDoubleClick(playerId);
        }
    };

    return (
        <WidgetRow onDoubleClick={() => handleDoubleClick(item.PlayerId)}>
            <GridRow>
                <PlayerName>
                    <DotIcon size={13} color={getConnectedColor(item.IsConnected)}/>
                    {item.PlayerName} {item.IsLeader ? <ChevronUpIcon size={15}/> : ""}
                </PlayerName>
                <Role>{item.Role}</Role>
            </GridRow>
            <ConstructRow construct={item.Construct} />
        </WidgetRow>
    );
}

export default PartyEntryMember;