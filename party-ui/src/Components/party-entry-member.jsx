import {AsteroidIcon, ChevronUpIcon, DotIcon, GlobeIcon} from "./icons";
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

const PartyEntryMember = ({item}) => {

    const getShieldColor = (value) => {
        if (value < 0.25) {
            return "rgb(250, 70, 70)";
        }

        return "rgb(0,255,255)";
    }

    const getCcsColor = (value) => {
        if (value < 0.25) {
            return "rgb(250, 70, 70)";
        }

        return "rgb(224,213,172)";
    };

    const getConnectedColor = (value) => {
        if (value) {
            return "rgb(0,255,0)";
        }

        return "rgb(0,0,0)";
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

    const last3Digits = (number) => {
        return number.toString().padStart(3, '0').slice(-3);
    }

    const percentValue = (value) => value * 100;

    if (!item) {
        return null;
    }

    const ConstructRow = ({construct}) => {
        if (!construct) {
            return null;
        }

        if (![3, 4, 5].includes(construct.ConstructKind))
        {
            return null;
        }

        return (
            <GridRow>
                <GridColShipName>
                    <ConstructSize>{getCoreSizeName(construct.Size)}</ConstructSize>
                    <ConstructName> - [{last3Digits(construct.ConstructId)}] "{construct.ConstructName}"</ConstructName>
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

    const AsteroidRow = ({construct}) => {

        if (!construct) {
            return null;
        }

        if (![2].includes(construct.ConstructKind))
        {
            return null;
        }

        return (
            <GridRow>
                <GridColShipName>
                    <ConstructName><AsteroidIcon size={14} />&nbsp;-&nbsp;{construct.ConstructName}</ConstructName>
                </GridColShipName>
            </GridRow>
        );
    }

    const PlanetRow = ({construct}) => {

        if (!construct) {
            return null;
        }

        if (![1].includes(construct.ConstructKind))
        {
            return null;
        }

        return (
            <GridRow>
                <GridColShipName>
                    <ConstructName><GlobeIcon size={13} />&nbsp;-&nbsp;{construct.ConstructName}</ConstructName>
                </GridColShipName>
            </GridRow>
        );
    }

    const handleDoubleClick = (playerId) => {
        window.modApi.setPlayerLocation(playerId);
    };

    return (
        <WidgetRow onDoubleClick={() => handleDoubleClick(item.PlayerId)}>
            <GridRow>
                <PlayerName>
                    <DotIcon size={12} color={getConnectedColor(item.IsConnected)}/>
                    &nbsp;&nbsp;{item.PlayerName} {item.IsLeader ? <ChevronUpIcon size={15}/> : ""}
                </PlayerName>
                <Role>{item.Role}</Role>
            </GridRow>
            <ConstructRow construct={item.Construct} />
            <AsteroidRow construct={item.Construct} />
            <PlanetRow construct={item.Construct} />
        </WidgetRow>
    );
}

export default PartyEntryMember;