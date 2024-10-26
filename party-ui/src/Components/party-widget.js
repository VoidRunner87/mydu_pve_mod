import styled from "styled-components";
import {useEffect, useState} from "react";
import {CaptainRankIcon, ChevronUpIcon, DotIcon} from "./icons";

//21333A
//304953

const Widget = styled.div`
    font-family: Play, Arial, Helvetica, sans-serif;
    border-top: 4px solid #B6DFED;
    color: white;
    //max-width: 14vw;
    background-color: rgba(13, 24, 28, 0.4);
`;

const WidgetHeader = styled.div`
    background: linear-gradient(to right, #21333A, #304953 50%, #21333A);
    text-align: center;
    text-transform: uppercase;
    padding: 8px;
`;

const WidgetContainer = styled.div`
    color: #ADD4E1;
    text-transform: uppercase;
    font-size: 0.75em;
    padding-bottom: 1px;
`;

const WidgetRow = styled.div`
    margin: 4px;
    padding: 8px;
    cursor: pointer;
    background-color: rgba(13, 24, 28, 1);
    border-radius: 2px;
    
    &:nth-child(even) {
        
    }
    
    &:hover {
        background-color: rgba(13, 24, 28, 0.5);
    }
`;

const PlayerName = styled.div`
    display: flex;
    flex-flow: row;
    flex-grow: 1;
`;

const Role = styled.div`
    display: flex;
    align-content: end;
`;

const GridRow = styled.div`
    display: flex;
    margin-bottom: 8px;

    &:last-child {
        margin-bottom: 0;
    }
`;

const GridRowBars = styled.div`
    display: flex;
    margin-bottom: 3px;

    &:last-child {
        margin-bottom: 0;
    }
`;


const ConstructName = styled.div`
    display: flex;
    flex-flow: row;
    flex-grow: 1;
    text-overflow: fade;
    white-space: nowrap;
    overflow: hidden;
    margin-right: 8px;
`;

const ConstructSize = styled.div`
    display: flex;
    font-weight: bold;
    margin-right: 4px;
`;

const PercentageBar = styled.div`
    width: 100%;
    height: 4px;
    background-color: rgba(13, 24, 28, 1);
    border-radius: 1px;
    overflow: hidden;
    position: relative;
`;

const FilledBar = styled.div`
    height: 100%;
    width: ${({percentage}) => `${percentage}%`}; /* Set width based on percentage prop */
    background-color: ${({color}) => `${color}`};
    transition: width 0.4s ease;
    border-radius: 1px 0 0 1px;
    background-image: repeating-linear-gradient(
            -90deg,
            rgba(13, 24, 28, 0.9) 0,
            rgba(13, 24, 28, 0.9) 1px,
            transparent 1px,
            transparent 15px
    );
    background-size: 15px 100%;
`;

const GridColShipName = styled.div`
    display: flex;
    width: 50%;
    align-items: center;
`;

const GridColBars = styled.div`
    width: 50%;
`;

const PartyEntries = ({data}) => {

    const getShieldColor = (value) => {
        if (value < 0.25) {
            return "#FF0000";
        }

        return "#00FFFF";
    }

    const getCcsColor = (value) => {
        if (value < 0.25) {
            return "#FF0000";
        }

        return "#E0D5AC";
    };

    const getConnectedColor = (value) => {
        if (value)
        {
            return "#00FF00";
        }

        return "#000000";
    };

    const getCoreSizeName = (size) => {
        switch (size){
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

    const entries = data.map((item, index) =>
        <WidgetRow key={index}>
            <GridRow>
                <PlayerName><DotIcon size={13} color={getConnectedColor(item.IsConnected)} /> {item.PlayerName} {item.IsLeader ? <ChevronUpIcon size={15}/>: ""}</PlayerName>
                <Role>{item.Role}</Role>
            </GridRow>
            <GridRow>
                <GridColShipName>
                    <ConstructSize>{getCoreSizeName(item.Construct.Size)}</ConstructSize>
                    <ConstructName> - [789] "{item.Construct.ConstructName}"</ConstructName>
                </GridColShipName>
                <GridColBars>
                    <GridRowBars>
                        <PercentageBar>
                            <FilledBar percentage={percentValue(item.Construct.ShieldRatio)} color={getShieldColor(item.Construct.ShieldRatio)}/>
                        </PercentageBar>
                    </GridRowBars>
                    <GridRowBars>
                        <PercentageBar>
                            <FilledBar percentage={percentValue(item.Construct.CoreStressRatio)} color={getCcsColor(item.Construct.CoreStressRatio)}/>
                        </PercentageBar>
                    </GridRowBars>
                </GridColBars>
            </GridRow>
        </WidgetRow>
    );

    return entries;
}

const PartyWidget = () => {

    const [data, setData] = useState([]);

    useEffect(() => {
        const url = window.global_resources["player-party"];

        fetch(url)
            .then(res => {
                return res.json()
            })
            .then(resJson => {
                console.log("data", resJson);

                let dataArray = [];
                dataArray.push(resJson.Leader);
                dataArray = dataArray.concat(resJson.Members);

                setData(dataArray);

                console.log("array", dataArray);
            });

    }, []);

    return (
        <Widget>
            <WidgetHeader>Group</WidgetHeader>
            <WidgetContainer>
                <PartyEntries data={data}/>
            </WidgetContainer>
        </Widget>
    );
};

export default PartyWidget;