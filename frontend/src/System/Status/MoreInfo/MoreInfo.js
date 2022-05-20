import React, { Component } from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItemDescription from 'Components/DescriptionList/DescriptionListItemDescription';
import DescriptionListItemTitle from 'Components/DescriptionList/DescriptionListItemTitle';
import FieldSet from 'Components/FieldSet';
import Link from 'Components/Link/Link';

class MoreInfo extends Component {

  //
  // Render

  render() {
    return (
      <FieldSet legend="More Info">
        <DescriptionList>
          <DescriptionListItemTitle>Home page</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://sonarr.tv/">sonarr.tv</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Wiki</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://wiki.servarr.com/sonarr">wiki.servarr.com/sonarr</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Forums</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://forums.sonarr.tv/">forums.sonarr.tv</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Twitter</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://twitter.com/sonarrtv">@sonarrtv</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Discord</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://discord.gg/73QUuf3bgA">discord.gg/73QUuf3bgA</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>IRC</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="irc://irc.libera.chat/#sonarr">#sonarr on Libera</Link>
          </DescriptionListItemDescription>
          <DescriptionListItemDescription>
            <Link to="https://web.libera.chat/?channels=#sonarr">Libera webchat</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Donations</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://sonarr.tv/donate">sonarr.tv/donate</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Source</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://github.com/Sonarr/Sonarr/">github.com/Sonarr/Sonarr</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Feature Requests</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://forums.sonarr.tv/">forums.sonarr.tv</Link>
          </DescriptionListItemDescription>
          <DescriptionListItemDescription>
            <Link to="https://github.com/Sonarr/Sonarr/issues">github.com/Sonarr/Sonarr/issues</Link>
          </DescriptionListItemDescription>

        </DescriptionList>
      </FieldSet>
    );
  }
}

MoreInfo.propTypes = {

};

export default MoreInfo;
